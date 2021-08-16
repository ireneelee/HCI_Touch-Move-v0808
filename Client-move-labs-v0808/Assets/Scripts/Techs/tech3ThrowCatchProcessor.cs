﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PublicInfo;
using static PublicDragParams;

public class tech3ThrowCatchProcessor : MonoBehaviour
{
    public lab1UIController uiController;
    public lab1TrialController trialController;

    public lab1TouchVisualizer touchVisualizer;
    public lab1TargetVisualizer targetVisualizer;
    public lab1FlickerVisualizer flickerVisualizer;

    private ThrowCatchStatus curTarget2ThrowCatchStatus, prevTarget2ThrowCatchStatus;
    private ThrowCatchResult curThrowCatchResult;

    private bool touchSuccess;
    //private ThrowCatchPhase1Result throwResult;
    private Vector3 throwStartPos, throwEndPos, moveStartPos, moveEndPos, actualEndPos;
    private float throwStartTime, throwEndTime, catchEndTime;
    private float thisMoveDuration;
    private Vector2 throwStartTouchPoint, throwEndTouchPoint;
    private float throwTouchVelocity, throwTouchDistance;
    private Vector3 prevMovingPos, curMovingPos;
    private Vector3 startTouchPointInWorld;

    private float leftBound;

    private const float unitMoveDuration = 0.3f; // The time the target moves one screen width as the unit time

    private const float minX = DRAG_MIN_X, maxX = DRAG_MAX_X, minY = DRAG_MIN_Y, maxY = DRAG_MAX_Y;
    private const float minFlickDistance = 60f;
    private const float junctionX = minX;

    private const float FLING_FRICTION = 1.1f;
    private const float FLING_MIN_VELOCITY = 200f; // ios 200
    private const float FLING_MIN_DISTANCE = 6f;  // ios 120

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ScreenHeight: " + Screen.height + "; ScreenWidth: " + Screen.width);
        leftBound = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        resetDirectDragParams();
    }

    void resetDirectDragParams()
    {
        touchSuccess = false;
        curThrowCatchResult = ThrowCatchResult.throw_catch_success;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.tech3Target1ThrowCatchResult
                = GlobalMemory.Instance.tech3Target2ThrowCatchResult
                = ThrowCatchResult.throw_catch_success;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalMemory.Instance.targetDragType == DragType.throw_catch)
        {
            if (curThrowCatchResult != ThrowCatchResult.throw_catch_success
                || GlobalMemory.Instance.tech3Target1ThrowCatchResult != ThrowCatchResult.throw_catch_success)
            {
                targetVisualizer.hideTarget();
                targetVisualizer.hideShadow();
                Debug.Log("Trial c failed: " + curThrowCatchResult.ToString());
                Debug.Log("Trial s failed: " + GlobalMemory.Instance.tech3Target1ThrowCatchResult.ToString());
                uiController.updatePosInfo(curThrowCatchResult.ToString());
                trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_failed_trial);
            }

            if (GlobalMemory.Instance.lab1Target2Status == TargetStatus.total_on_screen_1)
            {
                if (curTarget2ThrowCatchStatus == ThrowCatchStatus.inactive_on_screen_1
                    && GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.throw_successed_on_screen_1)
                {
                    flickerVisualizer.startFlicker();
                    targetVisualizer.moveTarget(GlobalMemory.Instance.tech3Target2ThrowCatchPosition);
                    uiController.updateStatusInfo(targetVisualizer.getTargetPosition().ToString());
                    curTarget2ThrowCatchStatus = ThrowCatchStatus.throw_successed_on_screen_1;
                }
                else if ((curTarget2ThrowCatchStatus == ThrowCatchStatus.throw_successed_on_screen_1 ||
                       curTarget2ThrowCatchStatus == ThrowCatchStatus.catch_start_on_screen_2)
                    && GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.throw_successed_on_screen_1)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        flickerVisualizer.stopFlicker();
                        flickerVisualizer.showFlickerObjects();
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.catch_start_on_screen_2;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        moveStartPos = targetVisualizer.getTargetPosition();
                        moveEndPos = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        moveStartPos = targetVisualizer.getTargetPosition();
                        moveEndPos = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                        catchEndTime = Time.time;
                        thisMoveDuration = (moveEndPos - moveStartPos).magnitude / (maxX - minX) * unitMoveDuration;
                        uiController.updatePosInfo(thisMoveDuration.ToString());
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.t1_move_phase2_ongoing;

                    }
#elif UNITY_IOS || UNITY_ANDROID
                    if ( Input.touchCount == 1 )
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began)
                        {
                            flickerVisualizer.stopFlicker();
                            flickerVisualizer.showFlickerObjects();
                            curTarget2ThrowCatchStatus = ThrowCatchStatus.catch_start_on_screen_2;
                        }
                        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            moveStartPos = targetVisualizer.getTargetPosition();
                            moveEndPos = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                        }
                        else if ( touch.phase == TouchPhase.Ended )
                        {
                            moveStartPos = targetVisualizer.getTargetPosition();
                            moveEndPos = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                            catchEndTime = Time.time;
                            thisMoveDuration = (moveEndPos - moveStartPos).magnitude / (maxX - minX) * unitMoveDuration;
                            uiController.updatePosInfo(thisMoveDuration.ToString());
                            curTarget2ThrowCatchStatus = ThrowCatchStatus.t1_move_phase2_ongoing;
                        }
                    } 
#endif
                    uiController.updateStatusInfo(moveStartPos.ToString());
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.t1_move_phase2_ongoing
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.t1_move_phase2_acrossing_over)
                {
                    float t = (Time.time - catchEndTime) / thisMoveDuration;
                    targetVisualizer.moveTarget(new Vector3(
                        Mathf.SmoothStep(moveStartPos.x, moveEndPos.x, t),
                        Mathf.SmoothStep(moveStartPos.y, moveEndPos.y, t),
                        0f));
                    curMovingPos = targetVisualizer.getTargetPosition();
                    targetVisualizer.activeTarget();
                    targetVisualizer.showTarget();
                    if (curMovingPos != prevMovingPos &&
                        targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2 <= leftBound)
                    {
                        prevMovingPos = curMovingPos;
                    }
                    else if (curMovingPos != prevMovingPos &&
                        targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2 > leftBound)
                    {
                        prevMovingPos = curMovingPos;
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.t1_move_phase2_acrossing_over;
                    }
                    else if (curMovingPos == prevMovingPos && curMovingPos.x == moveEndPos.x)
                    {
                        targetVisualizer.inactiveTarget();
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.catch_end_on_screen_2;
                        // send message later
                    }
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.catch_end_on_screen_2)
                {
                    flickerVisualizer.hideFlickerObjects();
                    targetVisualizer.inactiveTarget();
                    targetVisualizer.hideShadow();

                    if (checkTouchEndPosCorrect())
                    {
                        trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_successful_trial);
                    }
                    else
                    {
                        curThrowCatchResult = ThrowCatchResult.catch_failed_to_arrive_pos;
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.t1tot2_trial_failed;
                    }
                }
            }
            else if (GlobalMemory.Instance.lab1Target2Status == TargetStatus.total_on_screen_2)
            {
                if (curTarget2ThrowCatchStatus == ThrowCatchStatus.inactive_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        touchSuccess = process1Touch4Target2(Input.mousePosition, 0);
                        if (touchSuccess)
                        {
                            throwStartTime = Time.time;
                            throwStartPos = targetVisualizer.getTargetPosition();
                            throwStartTouchPoint = Input.mousePosition;
                            startTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            targetVisualizer.activeTarget();
                        }
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        if (touchSuccess)
                        {
                            Vector3 curTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            Vector3 offset = curTouchPointInWorld - startTouchPointInWorld;
                            Vector3 intentPos = throwStartPos + offset;
                            if (intentPos.x > minX && intentPos.x < maxX
                                && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(throwStartPos + offset);
                            }
                        }
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        if (touchSuccess)
                        {
                            Vector3 curTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            Vector3 offset = curTouchPointInWorld - startTouchPointInWorld;
                            Vector3 intentPos = throwStartPos + offset;
                            if (intentPos.x > minX && intentPos.x < maxX
                                && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(throwStartPos + offset);
                            }

                            throwEndTime = Time.time;
                            throwEndPos = targetVisualizer.getTargetPosition();
                            throwEndTouchPoint = Input.mousePosition;

                            throwTouchDistance = Mathf.Abs((throwEndTouchPoint - throwStartTouchPoint).magnitude);
                            throwTouchVelocity = throwTouchDistance / (throwEndTime - throwStartTime);

                            uiController.updateStatusInfo("PC-D/V:" + throwTouchDistance.ToString() + "/" + throwTouchVelocity.ToString());

                            if (throwTouchDistance > FLING_MIN_DISTANCE
                                && throwTouchVelocity > FLING_MIN_VELOCITY)
                            {
                                curTarget2ThrowCatchStatus = ThrowCatchStatus.throw_flicked_on_screen_2;
                            }
                            else if (throwTouchDistance <= FLING_MIN_DISTANCE
                                && throwTouchVelocity > FLING_MIN_VELOCITY)
                            {
                                targetVisualizer.inactiveTarget();
                                curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_d;
                                curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                            }
                            else if (throwTouchDistance > FLING_MIN_DISTANCE
                                && throwTouchVelocity <= FLING_MIN_VELOCITY)
                            {
                                targetVisualizer.inactiveTarget();
                                curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_v;
                                curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                            }
                            else
                            {
                                targetVisualizer.inactiveTarget();
                                curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_dv;
                                curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                    }
#elif UNITY_IOS || UNITY_ANDROID
                    if (Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began)
                        {
                            
                            touchSuccess = process1Touch4Target2(touch.position, 0);
                            if (touchSuccess)
                            {
                                throwStartTime = Time.time;
                                throwStartPos = targetVisualizer.getTargetPosition();
                                throwStartTouchPoint = touch.position;
                                startTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                                targetVisualizer.activeTarget();
                            }
                        }
                        else if (touch.phase == TouchPhase.Moved)
                        {
                            if (touchSuccess)
                            {
                                Vector3 curTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                                Vector3 offset = curTouchPointInWorld - startTouchPointInWorld;
                                Vector3 intentPos = throwStartPos + offset;
                                if (intentPos.x > minX && intentPos.x < maxX
                                    && intentPos.y > minY && intentPos.y < maxY)
                                {
                                    targetVisualizer.moveTarget(throwStartPos + offset);
                                }
                            }
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            if (touchSuccess)
                            {
                                Vector3 curTouchPointInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                                Vector3 offset = curTouchPointInWorld - startTouchPointInWorld;
                                Vector3 intentPos = throwStartPos + offset;
                                if (intentPos.x > minX && intentPos.x < maxX
                                    && intentPos.y > minY && intentPos.y < maxY)
                                {
                                    targetVisualizer.moveTarget(throwStartPos + offset);
                                }

                                throwEndTime = Time.time;
                                throwEndPos = targetVisualizer.getTargetPosition();
                                throwEndTouchPoint = touch.position;

                                //throwTouchDistance = Mathf.Abs((throwEndTouchPoint - throwStartTouchPoint).magnitude);
                                //throwTouchVelocity = throwTouchDistance / (throwEndTime - throwStartTime);
                                throwTouchDistance = touch.deltaPosition.magnitude;
                                throwTouchVelocity = throwTouchDistance / touch.deltaTime;

                                uiController.updateStatusInfo("TD-D/V:" + throwTouchDistance.ToString() + "/" + throwTouchVelocity.ToString());

                                if (throwTouchDistance > FLING_MIN_DISTANCE
                                    && throwTouchVelocity > FLING_MIN_VELOCITY)
                                {
                                    curTarget2ThrowCatchStatus = ThrowCatchStatus.throw_flicked_on_screen_2;
                                }
                                else if (throwTouchDistance <= FLING_MIN_DISTANCE
                                && throwTouchVelocity > FLING_MIN_VELOCITY)
                                {
                                    targetVisualizer.inactiveTarget();
                                    curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_d;
                                    curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                                }
                                else if (throwTouchDistance > FLING_MIN_DISTANCE
                                    && throwTouchVelocity <= FLING_MIN_VELOCITY)
                                {
                                    targetVisualizer.inactiveTarget();
                                    curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_v;
                                    curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                                }
                                else
                                {
                                    targetVisualizer.inactiveTarget();
                                    curThrowCatchResult = ThrowCatchResult.throw_downgraded_to_drag_due_dv;
                                    curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                                }
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                    }
#endif
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.throw_flicked_on_screen_2)
                {
                    moveStartPos = throwEndPos;
                    moveEndPos = calculate3rdPointOnLine(throwStartPos, throwEndPos);
                    prevMovingPos = curMovingPos = moveStartPos;
                    //thisMoveDuration = (moveEndPos - moveStartPos).magnitude / (maxX - minX) * unitMoveDuration;
                    Vector2 moveStartTouchPoint = uiController.renderCamera.WorldToScreenPoint(moveStartPos);
                    Vector2 moveEndTouchPoint = uiController.renderCamera.WorldToScreenPoint(moveEndPos);
                    float moveDistance = (moveEndTouchPoint - moveStartTouchPoint).magnitude;
                    thisMoveDuration = calculateMoveTime(-FLING_FRICTION, moveDistance, throwTouchVelocity);
                    uiController.updatePosInfo(thisMoveDuration.ToString());
                    curTarget2ThrowCatchStatus = ThrowCatchStatus.wait_for_t2_move_phase1;
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.wait_for_t2_move_phase1)
                {
                    float t = (Time.time - throwEndTime) / thisMoveDuration;
                    targetVisualizer.moveTarget(new Vector3(
                        Mathf.SmoothStep(moveStartPos.x, moveEndPos.x, t),
                        Mathf.SmoothStep(moveStartPos.y, moveEndPos.y, t),
                        0f));
                    curMovingPos = targetVisualizer.getTargetPosition();
                    if (curMovingPos != prevMovingPos)
                    {
                        prevMovingPos = curMovingPos;
                    }
                    else
                    {
                        if (curMovingPos.x == junctionX)
                        {
                            curTarget2ThrowCatchStatus = ThrowCatchStatus.throw_successed_on_screen_2;
                            // send message later
                        }
                        else
                        {
                            targetVisualizer.inactiveTarget();
                            curThrowCatchResult = ThrowCatchResult.throw_to_wrong_dir;
                            curTarget2ThrowCatchStatus = ThrowCatchStatus.t2tot1_trial_failed;
                        }
                    }
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.throw_successed_on_screen_2)
                {
                    flickerVisualizer.startFlicker();
                    curTarget2ThrowCatchStatus = ThrowCatchStatus.wait_for_catch_on_1;
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.wait_for_catch_on_1)
                {
                    if (GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.catch_start_on_screen_1
                        || GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.t2_move_phase2_ongoing
                        || GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.t2_move_phase2_acrossing_over
                        || GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.catch_end_on_screen_1)
                    {
                        flickerVisualizer.stopFlicker();
                        flickerVisualizer.showFlickerObjects();
                    }
                    if (GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.t2_move_phase2_ongoing)
                    {
                        targetVisualizer.moveTarget(GlobalMemory.Instance.tech3Target2ThrowCatchPosition);
                    }
                    else if (GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.t2_move_phase2_acrossing_over)
                    {
                        targetVisualizer.hideTarget();
                    }
                    else if (GlobalMemory.Instance.tech3Target1ThrowCatchStatus == ThrowCatchStatus.catch_end_on_screen_1)
                    {
                        targetVisualizer.hideTarget();
                        curTarget2ThrowCatchStatus = ThrowCatchStatus.catch_end_on_screen_1;
                    }
                }
                else if (curTarget2ThrowCatchStatus == ThrowCatchStatus.catch_end_on_screen_1)
                {
                    flickerVisualizer.hideFlickerObjects();
                    targetVisualizer.hideTarget();
                    targetVisualizer.hideShadow();
                    trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_successful_trial);
                }
            }

            uiController.updateDebugInfo(curTarget2ThrowCatchStatus.ToString());
            //uiController.updateStatusInfo(GlobalMemory.Instance.tech3Target1ThrowCatchStatus.ToString());
            //uiController.updatePosInfo(throwResult.ToString());
            GlobalMemory.Instance.tech3Target2ThrowCatchStatus = curTarget2ThrowCatchStatus;
            GlobalMemory.Instance.tech3Target2ThrowCatchPosition = targetVisualizer.getTargetPosition();
            GlobalMemory.Instance.tech3Target2ThrowCatchResult = curThrowCatchResult;
            // keep with t1 st-status
            if (curTarget2ThrowCatchStatus == ThrowCatchStatus.t1_move_phase2_ongoing
                || (curTarget2ThrowCatchStatus != prevTarget2ThrowCatchStatus &&
                     (curTarget2ThrowCatchStatus == ThrowCatchStatus.catch_start_on_screen_2
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.t1_move_phase2_acrossing_over
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.catch_end_on_screen_2
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.throw_successed_on_screen_2
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.t1tot2_trial_failed
                    || curTarget2ThrowCatchStatus == ThrowCatchStatus.t2tot1_trial_failed))
               )
            {
                GlobalMemory.Instance.client.prepareNewMessage4Server(MessageType.ThrowCatchInfo);
            }

            prevTarget2ThrowCatchStatus = curTarget2ThrowCatchStatus;
        }
    }
    public void initParamsWhenTargetOnScreen1(int id2)
    {
        Debug.Log("tech3-initS1()");
        targetVisualizer.moveShadowWithPosID(id2);
        targetVisualizer.hideTarget();
        targetVisualizer.showShadow();
        prevTarget2ThrowCatchStatus = curTarget2ThrowCatchStatus = ThrowCatchStatus.inactive_on_screen_1;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.tech3Target1ThrowCatchStatus
           = GlobalMemory.Instance.tech3Target2ThrowCatchStatus
           = ThrowCatchStatus.inactive_on_screen_1;
        }
        resetDirectDragParams();
    }
    public void initParamsWhenTargetOnScreen2(int id1)
    {
        Debug.Log("tech3-initS2()");
        targetVisualizer.moveTargetWithPosID(id1);
        targetVisualizer.showTarget();
        targetVisualizer.hideShadow();
        prevTarget2ThrowCatchStatus = curTarget2ThrowCatchStatus = ThrowCatchStatus.inactive_on_screen_2;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.tech3Target1ThrowCatchStatus
           = GlobalMemory.Instance.tech3Target2ThrowCatchStatus
           = ThrowCatchStatus.inactive_on_screen_2;
        }
        resetDirectDragParams();
    }

    private bool checkTouchEndPosCorrect()
    {
        bool res = false;
        float distance = Vector3.Distance(targetVisualizer.getTargetPosition(), targetVisualizer.getShadowPosition());
        if (distance <= targetVisualizer.getShadowLocalScale().x / 2)
        {
            res = true;
        }
        return res;
    }

    private Vector3 processScreenPosToGetWorldPosAtZeroZ(Vector2 tp)
    {
        Vector3 pos = Vector3.zero;
        pos = Camera.main.ScreenToWorldPoint(new Vector3(tp.x, tp.y, 0));
        pos.z = 0f;
        return pos;
    }

    private bool process1Touch4Target2(Vector2 pos, int targetid)
    {
        int hitid = -1;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out hit))
        {
            hitid = Convert.ToInt32(hit.collider.gameObject.name.Substring(7, 2));
            Debug.Log("info: " + hitid.ToString() + " " + hit.collider.gameObject.name);
            Debug.DrawLine(ray.origin, hit.point, Color.yellow);
        }

        if (hitid == targetid)
            return true;
        else
            return false;

    }

    private Vector3 calculate3rdPointOnLine(Vector2 p1, Vector2 p2)
    {
        // p1-start, p2-end
        Vector3 res = Vector3.zero;
        bool isMovingRight = p2.x - p1.x > 0;
        bool isMovingDown = p2.y - p1.y > 0;
        if (p2.x - p1.x == 0 && p2.y - p1.y == 0)
        {
            // Impossible condition
            res.x = p1.x;
            res.y = p2.y;
        }
        else if (p2.x - p1.x == 0 && isMovingDown)
        {
            res.x = p1.x;
            res.y = minY;
        }
        else if (p2.x - p1.x == 0 && !isMovingDown)
        {
            res.x = p1.x;
            res.y = maxY;
        }
        else if (p2.y - p1.y == 0 && isMovingRight)
        {
            res.x = maxX;
            res.y = p1.y;
        }
        else if (p2.y - p1.y == 0 && !isMovingRight)
        {
            res.x = minX;
            res.y = p1.y;
        }
        else if (isMovingRight)
        {
            res.x = maxX;
            res.y = (res.x - p1.x) * (p2.y - p1.y) / (p2.x - p1.x) + p1.y;

        }
        else if (!isMovingRight)
        {
            res.x = minX;
            res.y = (res.x - p1.x) * (p2.y - p1.y) / (p2.x - p1.x) + p1.y;
        }

        if (res.y > maxY)
        {
            res.y = maxY;
            res.x = (res.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
        }
        else if (res.y < minY)
        {
            res.y = minY;
            res.x = (res.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
        }
        return res;
    }

    private float calculateMoveTime(float a, float s, float v0)
    {
        float coe_a = 0.5f * a;
        float coe_b = v0;
        float coe_c = -s;

        float x1 = 0f, x2 = 0f;
        if (coe_b * coe_b - 4 * coe_a * coe_c > 0)
        {
            x1 = (-coe_b + Mathf.Sqrt(coe_b * coe_b - 4f * coe_a * coe_c)) / 2f * coe_a;
            x2 = (-coe_b - Mathf.Sqrt(coe_b * coe_b - 4f * coe_a * coe_c)) / 2f * coe_a;
            Debug.Log(String.Format("一元二次方程{0}*x*x+{1}*x+{2}=0的根为：{3}\t{4}", coe_a, coe_b, coe_c, x1, x2));
        }
        else if (coe_b * coe_b - 4f * coe_a * coe_c == 0f)
        {
            x1 = (-coe_b + Mathf.Sqrt(coe_b * coe_b - 4f * coe_a * coe_c)) / 2f * coe_a;
            Debug.Log(String.Format("一元二次方程{0}*x*x+{1}*x+{2}=0的根为：{3}", coe_a, coe_b, coe_c, x1));
        }
        else
        {
            Debug.Log(String.Format("一元二次方程{0}*x*x+{1}*x+{2}=0无解！", coe_a, coe_b, coe_c));
        }

        if (x1 > 0) return x1;
        if (x2 > 0) return x2;
        return 0f;
    }
}