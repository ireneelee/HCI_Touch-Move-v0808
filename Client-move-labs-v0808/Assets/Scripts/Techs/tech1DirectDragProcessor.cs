﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PublicInfo;
using static PublicDragParams;

public class tech1DirectDragProcessor : MonoBehaviour
{
    public lab1UIController uiController;
    public lab1TrialController trialController;
    public lab1TargetVisualizer targetVisualizer;

    private int firstid, secondid;

    private DirectDragStatus curTarget2DirectDragStatus, prevTarget2DirectDragStatus;
    private Vector3 prevTarget2Pos, curTarget2Pos;

    private bool touchSuccess;
    private Vector3 dragStartTouchPosInWorld;
    private Vector3 dragStartTargetPos;

    private DirectDragResult curDirectDragResult;

    private float leftBound;
    private const float minX = DRAG_MIN_X, maxX = DRAG_MAX_X, minY = DRAG_MIN_Y, maxY = DRAG_MAX_Y;

    private float delayTimer = 0f;
    private const float wait_time_before_vanish = 0.2f;

    private bool haveRecordedStamp = false;
    private bool phase2FirstTouchHappened = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ScreenHeight: " + Screen.height + "; ScreenWidth: " + Screen.width);
        leftBound = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        resetDirectDragParams();
    }

    void resetDirectDragParams()
    {
        delayTimer = wait_time_before_vanish;
        haveRecordedStamp = false;
        phase2FirstTouchHappened = false;
        touchSuccess = false;
        curDirectDragResult = DirectDragResult.direct_drag_success;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.tech1Target1DirectDragResult
                = GlobalMemory.Instance.tech1Target2DirectDragResult
                = DirectDragResult.direct_drag_success;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalMemory.Instance && GlobalMemory.Instance.targetDragType == DragType.direct_drag)
        {

            if (curDirectDragResult != DirectDragResult.direct_drag_success
                || GlobalMemory.Instance.tech1Target1DirectDragResult != DirectDragResult.direct_drag_success)
            {
                Debug.Log("Trial c failed: " + curDirectDragResult.ToString());
                Debug.Log("Trial s failed: " + GlobalMemory.Instance.tech1Target1DirectDragResult.ToString());
                uiController.updatePosInfo(curDirectDragResult.ToString());
                targetVisualizer.wrongTarget();
                if (delayTimer > 0f)
                {
                    delayTimer -= Time.deltaTime;
                }
                else
                {
                    targetVisualizer.hideTarget();
                    targetVisualizer.hideShadow();
                    trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_failed_trial);
                }
            }

            if (GlobalMemory.Instance.lab1Target2Status == TargetStatus.total_on_screen_1)
            {
                if (curTarget2DirectDragStatus == DirectDragStatus.inactive_on_screen_1
                    && (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.across_from_screen_1
                        || GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase1_end_on_screen_1))
                {
                    if (GlobalMemory.Instance.refreshTarget2)
                    {
                        targetVisualizer.moveTarget(GlobalMemory.Instance.tech1Target2DirectDragPosition);
                        targetVisualizer.activeTarget();
                        //targetVisualizer.zoominTarget();
                        targetVisualizer.showTarget();
                        GlobalMemory.Instance.refreshTarget2 = false;
                    }
                    if (!haveRecordedStamp)
                    {
                        GlobalMemory.Instance.curLabPhase2RawData.targetReachMidpointInfoReceivedStamp = CurrentTimeMillis();
                        GlobalMemory.Instance.curLabPhase2RawData.movePhase2StartPos = targetVisualizer.getTargetScreenPosition();
                        haveRecordedStamp = true;
                    }
                    if (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase1_end_on_screen_1)
                    {
                        targetVisualizer.inactiveTarget();
                    }
#if UNITY_ANDROID && UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                    {
                        touchSuccess = process1Touch4Target2(Input.mousePosition, 0);
                        if (touchSuccess)
                        {
                            GlobalMemory.Instance.curLabPhase2RawData.touch2StartStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase2RawData.touch2StartPos = Input.mousePosition;
                            if (!phase2FirstTouchHappened)
                            {
                                GlobalMemory.Instance.tech1TrialData.device2FirstTouchPosition
                                    = GlobalMemory.Instance.tech1TrialData.device2FirstCorrectPosition
                                    = Input.mousePosition;
                                GlobalMemory.Instance.tech1TrialData.device2FirstTouchStamp
                                    = GlobalMemory.Instance.tech1TrialData.device2FirstCorrectTouchStamp
                                    = CurrentTimeMillis();
                            }
                            else
                            {
                                GlobalMemory.Instance.tech1TrialData.device2FirstCorrectPosition
                                    = Input.mousePosition;
                                GlobalMemory.Instance.tech1TrialData.device2FirstCorrectTouchStamp
                                    = CurrentTimeMillis();
                            }
                            targetVisualizer.activeTarget();
                            dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            dragStartTargetPos = targetVisualizer.getTargetPosition();
                            curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_on_screen_2;
                        }
                        else if (!phase2FirstTouchHappened)
                        {
                            GlobalMemory.Instance.tech1TrialData.device2FirstTouchPosition = Input.mousePosition;
                            GlobalMemory.Instance.tech1TrialData.device2FirstTouchStamp = CurrentTimeMillis();
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
                        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            touchSuccess = process1Touch4Target2(touch.position, 0);
                            if (touchSuccess)
                            {
                                GlobalMemory.Instance.curLabPhase2RawData.touch2StartStamp = CurrentTimeMillis();
                                GlobalMemory.Instance.curLabPhase2RawData.touch2StartPos = touch.position;
                                if (!phase2FirstTouchHappened)
                                {
                                    GlobalMemory.Instance.tech1TrialData.device2FirstTouchPosition
                                        = GlobalMemory.Instance.tech1TrialData.device2FirstCorrectPosition
                                        = touch.position;
                                    GlobalMemory.Instance.tech1TrialData.device2FirstTouchStamp
                                        = GlobalMemory.Instance.tech1TrialData.device2FirstCorrectTouchStamp
                                        = CurrentTimeMillis();
                                }
                                else
                                {
                                    GlobalMemory.Instance.tech1TrialData.device2FirstCorrectPosition
                                        = touch.position;
                                    GlobalMemory.Instance.tech1TrialData.device2FirstCorrectTouchStamp
                                        = CurrentTimeMillis();
                                }
                                targetVisualizer.activeTarget();
                                dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                                dragStartTargetPos = targetVisualizer.getTargetPosition();
                                curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_on_screen_2;
                            }
                            else if (!phase2FirstTouchHappened)
                            {
                                GlobalMemory.Instance.tech1TrialData.device2FirstTouchPosition = touch.position;
                                GlobalMemory.Instance.tech1TrialData.device2FirstTouchStamp = CurrentTimeMillis();
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                    }
#endif
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (touchSuccess && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
                    {
                        Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                        Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                        Vector3 intentPos = dragStartTargetPos + offset;
                        if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                        {
                            targetVisualizer.moveTarget(intentPos);
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            GlobalMemory.Instance.curLabPhase2RawData.touch2EndStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase2RawData.touch2EndPos = Input.mousePosition;
                            GlobalMemory.Instance.curLabPhase2RawData.movePhase2EndPos = targetVisualizer.getTargetScreenPosition();
                            targetVisualizer.inactiveTarget();
                            targetVisualizer.wrongTarget();
                            curDirectDragResult = DirectDragResult.drag_2_failed_to_leave_junction;
                            curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                        }
                    }
#elif UNITY_IOS || UNITY_ANDROID
                    if (touchSuccess && Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved
                            || touch.phase == TouchPhase.Ended)
                        {
                            Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                            Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                            Vector3 intentPos = dragStartTargetPos + offset;
                            if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(intentPos);
                            }
                            if (touch.phase == TouchPhase.Ended)
                            {
                                GlobalMemory.Instance.curLabPhase2RawData.touch2EndStamp = CurrentTimeMillis();
                                GlobalMemory.Instance.curLabPhase2RawData.touch2EndPos = touch.position;
                                GlobalMemory.Instance.curLabPhase2RawData.movePhase2EndPos = targetVisualizer.getTargetScreenPosition();
                                targetVisualizer.inactiveTarget();
                                targetVisualizer.wrongTarget();
                                curDirectDragResult = DirectDragResult.drag_2_failed_to_leave_junction;
                                curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                            }
                        }
                    }
#endif
                    if ( (targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) > leftBound)
                    {
                        //targetVisualizer.zoomoutTarget();
                        curTarget2DirectDragStatus = DirectDragStatus.across_end_from_screen_1;
                    }
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.across_end_from_screen_1
                    || curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_ongoing_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (touchSuccess && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
                    {
                        Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                        Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                        Vector3 intentPos = dragStartTargetPos + offset;
                        if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                        {
                            targetVisualizer.moveTarget(intentPos);
                        }
                        if (curTarget2DirectDragStatus == DirectDragStatus.across_end_from_screen_1)
                        {
                            curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_ongoing_on_screen_2;
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            GlobalMemory.Instance.curLabPhase2RawData.touch2EndStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase2RawData.touch2EndPos = Input.mousePosition;
                            GlobalMemory.Instance.curLabPhase2RawData.movePhase2EndPos = targetVisualizer.getTargetScreenPosition();
                            GlobalMemory.Instance.curLabPhase2RawData.targetReachEndpointStamp = CurrentTimeMillis();
                            targetVisualizer.inactiveTarget();
                            if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                            {
                                targetVisualizer.wrongTarget();
                                curDirectDragResult = DirectDragResult.drag_2_rearrived_junction_after_leave;
                                curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                            }
                            else
                            {
                                if (checkTouchEndPosCorrect())
                                {
                                    targetVisualizer.correctTarget();
                                    curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_end_on_screen_2;
                                }
                                else
                                {
                                    targetVisualizer.wrongTarget();
                                    curDirectDragResult = DirectDragResult.drag_2_failed_to_arrive_pos;
                                    curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                                }
                            }
                        }
                    }
                    /*else
                    {
                        touchSuccess = false;
                    }*/
#elif UNITY_IOS || UNITY_ANDROID
                    if (touchSuccess && Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary
                            || touch.phase == TouchPhase.Ended)
                        {

                            Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                            Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                            Vector3 intentPos = dragStartTargetPos + offset;
                            if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(intentPos);
                            }
                            if (curTarget2DirectDragStatus == DirectDragStatus.across_end_from_screen_1)
                            {
                                curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_ongoing_on_screen_2;
                            }
                            if (touch.phase == TouchPhase.Ended)
                            {
                                GlobalMemory.Instance.curLabPhase2RawData.touch2EndStamp = CurrentTimeMillis();
                                GlobalMemory.Instance.curLabPhase2RawData.touch2EndPos = touch.position;
                                GlobalMemory.Instance.curLabPhase2RawData.movePhase2EndPos = targetVisualizer.getTargetScreenPosition();
                                GlobalMemory.Instance.curLabPhase2RawData.targetReachEndpointStamp = CurrentTimeMillis();
                                targetVisualizer.inactiveTarget();
                                if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                                {
                                    targetVisualizer.wrongTarget();
                                    curDirectDragResult = DirectDragResult.drag_2_rearrived_junction_after_leave;
                                    curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                                }
                                else
                                {
                                    if (checkTouchEndPosCorrect())
                                    {
                                        targetVisualizer.correctTarget();
                                        curTarget2DirectDragStatus = DirectDragStatus.drag_phase2_end_on_screen_2;
                                    }
                                    else
                                    {
                                        targetVisualizer.wrongTarget();
                                        curDirectDragResult = DirectDragResult.drag_2_failed_to_arrive_pos;
                                        curTarget2DirectDragStatus = DirectDragStatus.t1tot2_trial_failed;
                                    }
                                }
                            }
                        }
                    }
                    /*else
                    {
                        touchSuccess = false;
                    }*/
#endif
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_end_on_screen_2)
                {
                    if (delayTimer > 0f)
                    {
                        delayTimer -= Time.deltaTime;
                    }
                    else
                    {
                        targetVisualizer.hideTarget();
                        targetVisualizer.hideShadow();
                        uiController.updatePosInfo(curDirectDragResult.ToString());
                        trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_successful_trial);
                    }
                }
            }
            else if (GlobalMemory.Instance.lab1Target2Status == TargetStatus.total_on_screen_2)
            {
                if (curTarget2DirectDragStatus == DirectDragStatus.inactive_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        touchSuccess = process1Touch4Target2(Input.mousePosition, 0);
                        if (touchSuccess)
                        {
                            GlobalMemory.Instance.curLabPhase1RawData.touch1StartStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase1RawData.touch1StartPos = Input.mousePosition;
                            targetVisualizer.activeTarget();
                            dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            dragStartTargetPos = targetVisualizer.getTargetPosition();
                            curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_on_screen_2;
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
                                GlobalMemory.Instance.curLabPhase1RawData.touch1StartStamp = CurrentTimeMillis();
                                GlobalMemory.Instance.curLabPhase1RawData.touch1StartPos = touch.position;
                                targetVisualizer.activeTarget();
                                dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                                dragStartTargetPos = targetVisualizer.getTargetPosition();
                                curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_on_screen_2;
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                    }
#endif
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.drag_phase1_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (touchSuccess && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) )
                    {
                        Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                        Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                        Vector3 intentPos = dragStartTargetPos + offset;
                        if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                        {
                            targetVisualizer.moveTarget(intentPos);
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            GlobalMemory.Instance.curLabPhase1RawData.touch1EndStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase1RawData.touch1EndPos = Input.mousePosition;
                            GlobalMemory.Instance.curLabPhase1RawData.movePhase1EndPos = targetVisualizer.getTargetScreenPosition();
                            targetVisualizer.inactiveTarget();
                            targetVisualizer.wrongTarget();
                            // record wrong touch position
                            curDirectDragResult = DirectDragResult.drag_1_failed_to_arrive_junction;
                            curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                        targetVisualizer.inactiveTarget();
                        targetVisualizer.wrongTarget();
                        // record wrong touch position
                        curDirectDragResult = DirectDragResult.drag_1_failed_to_arrive_junction;
                        curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                    }
#elif UNITY_IOS || UNITY_ANDROID
                    if (touchSuccess && Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary
                            || touch.phase == TouchPhase.Ended)
                        {
                            Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                            Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                            Vector3 intentPos = dragStartTargetPos + offset;
                            if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(intentPos);
                            }
                            if (touch.phase == TouchPhase.Ended)
                            {
                                GlobalMemory.Instance.curLabPhase1RawData.touch1EndStamp = CurrentTimeMillis();
                                GlobalMemory.Instance.curLabPhase1RawData.touch1EndPos = touch.position;
                                GlobalMemory.Instance.curLabPhase1RawData.movePhase1EndPos = targetVisualizer.getTargetScreenPosition();
                                targetVisualizer.inactiveTarget();
                                targetVisualizer.wrongTarget();
                                // record wrong touch position
                                curDirectDragResult = DirectDragResult.drag_1_failed_to_arrive_junction;
                                curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                        targetVisualizer.inactiveTarget();
                        targetVisualizer.wrongTarget();
                        // record wrong touch position
                        curDirectDragResult = DirectDragResult.drag_1_failed_to_arrive_junction;
                        curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                    }
#endif
                    if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                    {
                        //targetVisualizer.zoominTarget();
                        GlobalMemory.Instance.curLabPhase1RawData.targetReachMidpointStamp = CurrentTimeMillis();
                        curTarget2DirectDragStatus = DirectDragStatus.across_from_screen_2;
                    }
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.across_from_screen_2)
                {
                    if (GlobalMemory.Instance.refreshTarget2
                        && GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_1)
                    {
                        targetVisualizer.activeTarget();
                        targetVisualizer.moveTarget(GlobalMemory.Instance.tech1Target2DirectDragPosition);
                        GlobalMemory.Instance.refreshTarget2 = false;
                        curTarget2DirectDragStatus = DirectDragStatus.wait_for_drag_on_1;
                    }
#if UNITY_ANDROID && UNITY_EDITOR
                    else if (touchSuccess && (Input.GetMouseButtonUp(0) || Input.GetMouseButton(0)) )
                    {
                        Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                        Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                        Vector3 intentPos = dragStartTargetPos + offset;
                        if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                        {
                            targetVisualizer.moveTarget(intentPos);
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            targetVisualizer.inactiveTarget();
                        }
                        GlobalMemory.Instance.curLabPhase1RawData.touch1EndStamp = CurrentTimeMillis();
                        GlobalMemory.Instance.curLabPhase1RawData.touch1EndPos = Input.mousePosition;
                        GlobalMemory.Instance.curLabPhase1RawData.movePhase1EndPos = targetVisualizer.getTargetScreenPosition();
                        if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                        {
                            //GlobalMemory.Instance.curLabPhase1RawData.targetReachMidpointStamp = CurrentTimeMillis();
                            //curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_end_on_screen_2;
                        }
                        else
                        {
                            targetVisualizer.wrongTarget();
                            curDirectDragResult = DirectDragResult.drag_1_left_junction_after_arrive;
                            curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                        targetVisualizer.inactiveTarget();
                        if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                        {
                            //GlobalMemory.Instance.curLabPhase1RawData.targetReachMidpointStamp = CurrentTimeMillis();
                            curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_end_on_screen_2;
                        }
                        else
                        {
                            targetVisualizer.wrongTarget();
                            curDirectDragResult = DirectDragResult.drag_1_left_junction_after_arrive;
                            curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                        }
                    }
#elif UNITY_IOS || UNITY_ANDROID
                    else if (touchSuccess && Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary
                            || touch.phase == TouchPhase.Ended)
                        {
                            Vector3 curTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(touch.position);
                            Vector3 offset = curTouchPosInWorld - dragStartTouchPosInWorld;
                            Vector3 intentPos = dragStartTargetPos + offset;
                            if (intentPos.x < maxX && intentPos.y > minY && intentPos.y < maxY)
                            {
                                targetVisualizer.moveTarget(intentPos);
                            }
                            if (Input.GetMouseButtonUp(0))
                            {
                                targetVisualizer.inactiveTarget();
                            } 
                            GlobalMemory.Instance.curLabPhase1RawData.touch1EndStamp = CurrentTimeMillis();
                            GlobalMemory.Instance.curLabPhase1RawData.touch1EndPos = touch.position;
                            GlobalMemory.Instance.curLabPhase1RawData.movePhase1EndPos = targetVisualizer.getTargetScreenPosition();
                            if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                            {
                                //GlobalMemory.Instance.curLabPhase1RawData.targetReachMidpointStamp = CurrentTimeMillis();
                                //curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_end_on_screen_2;
                            }
                            else
                            {
                                targetVisualizer.wrongTarget();
                                curDirectDragResult = DirectDragResult.drag_1_left_junction_after_arrive;
                                curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                            }
                        }
                    }
                    else
                    {
                        touchSuccess = false;
                        targetVisualizer.inactiveTarget();
                        if ((targetVisualizer.getTargetPosition().x - targetVisualizer.getTargetLocalScale().x / 2) < leftBound)
                        {
                            //GlobalMemory.Instance.curLabPhase1RawData.targetReachMidpointStamp = CurrentTimeMillis();
                            curTarget2DirectDragStatus = DirectDragStatus.drag_phase1_end_on_screen_2;
                        }
                        else
                        {
                            targetVisualizer.wrongTarget();
                            curDirectDragResult = DirectDragResult.drag_1_left_junction_after_arrive;
                            curTarget2DirectDragStatus = DirectDragStatus.t2tot1_trial_failed;
                        }
                    }
#endif
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.drag_phase1_end_on_screen_2)
                {
#if UNITY_ANDROID && UNITY_EDITOR
                    if (Input.GetMouseButtonDown(0))
                    {
                        touchSuccess = process1Touch4Target2(Input.mousePosition, 0);
                        if (touchSuccess)
                        {
                            targetVisualizer.activeTarget();
                            dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.mousePosition);
                            dragStartTargetPos = targetVisualizer.getTargetPosition();
                            curTarget2DirectDragStatus = DirectDragStatus.across_from_screen_2;
                        }
                    }
#elif UNITY_IOS || UNITY_ANDROID
                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        touchSuccess = process1Touch4Target2(Input.GetTouch(0).position, 0);
                        if (touchSuccess)
                        {
                            //GlobalMemory.Instance.curLabPhase1RawData.touch1StartStamp = CurrentTimeMillis();
                            //GlobalMemory.Instance.curLabPhase1RawData.touch1StartPos = Input.mousePosition;
                            targetVisualizer.activeTarget();
                            dragStartTouchPosInWorld = processScreenPosToGetWorldPosAtZeroZ(Input.GetTouch(0).position);
                            dragStartTargetPos = targetVisualizer.getTargetPosition();
                            curTarget2DirectDragStatus = DirectDragStatus.across_from_screen_2;
                        }
                    }

#endif
                    if (GlobalMemory.Instance.refreshTarget2
                        && GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_1)
                    {
                        targetVisualizer.activeTarget();
                        targetVisualizer.moveTarget(GlobalMemory.Instance.tech1Target2DirectDragPosition);
                        GlobalMemory.Instance.refreshTarget2 = false;
                        curTarget2DirectDragStatus = DirectDragStatus.wait_for_drag_on_1;
                    }
                    if (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.across_end_from_screen_2)
                    {
                        targetVisualizer.hideTarget();
                        //targetVisualizer.zoomoutTarget();
                    }
                    else if (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase2_end_on_screen_1)
                    {
                        targetVisualizer.hideTarget();
                        targetVisualizer.hideShadow();
                        GlobalMemory.Instance.curLabPhase1RawData.targetReachEndpointInfoReceivedStamp = CurrentTimeMillis();
                        uiController.updatePosInfo(curDirectDragResult.ToString());
                        trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_successful_trial);
                    }
                }
                else if (curTarget2DirectDragStatus == DirectDragStatus.wait_for_drag_on_1)
                {
                    if (GlobalMemory.Instance.refreshTarget2
                        && GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_1)
                    {
                        targetVisualizer.activeTarget();
                        targetVisualizer.moveTarget(GlobalMemory.Instance.tech1Target2DirectDragPosition);
                        GlobalMemory.Instance.refreshTarget2 = false;
                    }
                    if (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.across_end_from_screen_2)
                    {
                        targetVisualizer.hideTarget();
                        //targetVisualizer.zoomoutTarget();
                    }
                    else if (GlobalMemory.Instance.tech1Target1DirectDragStatus == DirectDragStatus.drag_phase2_end_on_screen_1)
                    {
                        targetVisualizer.hideTarget();
                        targetVisualizer.hideShadow();
                        GlobalMemory.Instance.curLabPhase1RawData.targetReachEndpointInfoReceivedStamp = CurrentTimeMillis();
                        uiController.updatePosInfo(curDirectDragResult.ToString());
                        trialController.switchTrialPhase(PublicTrialParams.TrialPhase.a_successful_trial);
                    }
                }
            }

            curTarget2Pos = targetVisualizer.getTargetPosition();
            GlobalMemory.Instance.tech1Target2DirectDragPosition = curTarget2Pos;
            GlobalMemory.Instance.tech1Target2DirectDragStatus = curTarget2DirectDragStatus;
            GlobalMemory.Instance.tech1Target2DirectDragResult = curDirectDragResult;
            if ((curTarget2DirectDragStatus == DirectDragStatus.across_from_screen_2 && prevTarget2Pos != curTarget2Pos)
                || (curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_2 && prevTarget2Pos != curTarget2Pos)
                || ((curTarget2DirectDragStatus != prevTarget2DirectDragStatus) &&
                (curTarget2DirectDragStatus == DirectDragStatus.drag_phase1_end_on_screen_2
                || curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_on_screen_2
                || curTarget2DirectDragStatus == DirectDragStatus.across_end_from_screen_1
                || curTarget2DirectDragStatus == DirectDragStatus.drag_phase2_end_on_screen_2
                || curTarget2DirectDragStatus == DirectDragStatus.t1tot2_trial_failed
                || curTarget2DirectDragStatus == DirectDragStatus.t2tot1_trial_failed) ) )
            {
                GlobalMemory.Instance.client.prepareNewMessage4Server(MessageType.DirectDragInfo);
            }
            prevTarget2DirectDragStatus = curTarget2DirectDragStatus;
            prevTarget2Pos = curTarget2Pos;

            uiController.updateDebugInfo(curTarget2DirectDragStatus.ToString());
            uiController.updateStatusInfo(GlobalMemory.Instance.tech1Target1DirectDragStatus.ToString());
            uiController.updatePosInfo(touchSuccess.ToString());
        }
    }

    private long CurrentTimeMillis()
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
        return (long)diff.TotalMilliseconds;
    }

    private bool process1Touch4Target2(Vector2 pos, int targetid)
    {
        /*
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
        */
        float distance = Vector3.Distance(targetVisualizer.getTargetPosition(), processScreenPosToGetWorldPosAtZeroZ(pos));
        if (distance <= targetVisualizer.getShadowLocalScale().x / 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 processScreenPosToGetWorldPosAtZeroZ(Vector2 tp)
    {
        Vector3 pos = Vector3.zero;
        pos = Camera.main.ScreenToWorldPoint(new Vector3(tp.x, tp.y, 0));
        pos.z = 0f;
        return pos;
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

    public void initParamsWhenTargetOnScreen1 (int id2)
    {
        Debug.Log("tech1-initS1()");
        secondid = id2;
        targetVisualizer.moveShadowWithPosID(id2);
        targetVisualizer.hideTarget();
        targetVisualizer.showShadow();
        prevTarget2DirectDragStatus = curTarget2DirectDragStatus = DirectDragStatus.inactive_on_screen_1;
        curDirectDragResult = DirectDragResult.direct_drag_success;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.curLabPhase2RawData.moveDestination = targetVisualizer.getShadowScreenPosition();
            GlobalMemory.Instance.tech1Target1DirectDragStatus
                = GlobalMemory.Instance.tech1Target2DirectDragStatus
                = DirectDragStatus.inactive_on_screen_1;
        }
        resetDirectDragParams();
    }
    public void initParamsWhenTargetOnScreen2 (int id1)
    {
        Debug.Log("tech1-initS2()");
        firstid = id1;
        targetVisualizer.moveTargetWithPosID(id1);
        targetVisualizer.showTarget();
        targetVisualizer.hideShadow();
        prevTarget2DirectDragStatus = curTarget2DirectDragStatus = DirectDragStatus.inactive_on_screen_2;
        prevTarget2Pos = curTarget2Pos = targetVisualizer.getTargetPosition();
        curDirectDragResult = DirectDragResult.direct_drag_success;
        if (GlobalMemory.Instance)
        {
            GlobalMemory.Instance.curLabPhase1RawData.moveStartPos
                = GlobalMemory.Instance.curLabPhase1RawData.movePhase1StartPos
                = targetVisualizer.getTargetScreenPosition();
            GlobalMemory.Instance.tech1Target1DirectDragStatus
                = GlobalMemory.Instance.tech1Target2DirectDragStatus
                = DirectDragStatus.inactive_on_screen_2;
            GlobalMemory.Instance.tech1Target2DirectDragPosition = curTarget2Pos;
        }
        resetDirectDragParams();
    }
}
