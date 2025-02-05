﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicDragParams : MonoBehaviour
{
    //private static char paramSeperators = ';';
    //private static char posSeperator = '*';
    private static char techParamSeperator = '#';
    public const float DRAG_MIN_X = -3.5f, DRAG_MAX_X = 3.5f, DRAG_MIN_Y = -9f, DRAG_MAX_Y = 9f;

    public enum TargetStatus
    {
        total_on_screen_1 = 0,
        total_on_screen_2 = 1,
    }

    public enum DirectDragStatus
    {
        inactive_on_screen_1 = 10,
        drag_phase1_on_screen_1 = 11,
        across_from_screen_1 = 12,
        drag_phase1_end_on_screen_1 = 13,
        wait_for_drag_on_2 = 14,
        drag_phase2_on_screen_2 = 15,
        across_end_from_screen_1 = 16,
        drag_phase2_ongoing_on_screen_2 = 17,
        drag_phase2_end_on_screen_2 = 18,
        t1tot2_trial_failed = 19,

        inactive_on_screen_2 = 30,
        drag_phase1_on_screen_2 = 31,
        across_from_screen_2 = 32,
        drag_phase1_end_on_screen_2 = 33,
        wait_for_drag_on_1 = 34,
        drag_phase2_on_screen_1 = 35,
        across_end_from_screen_2 = 36,
        drag_phase2_ongoing_on_screen_1 = 37,
        drag_phase2_end_on_screen_1 = 38,
        t2tot1_trial_failed = 39,

        retry_status_active = 100,
    }

    public enum DirectDragResult
    {
        direct_drag_success = 0,
        drag_1_failed_to_arrive_junction = 1,
        drag_1_left_junction_after_arrive = 2,
        //drag_2_failed_to_touch = 3,
        drag_2_failed_to_leave_junction = 4,
        drag_2_rearrived_junction_after_leave = 5,
        drag_2_failed_to_arrive_pos = 6,

        user_skip_current_trial = 100,
    }

    public enum HoldTapStatus
    {
        inactive_on_screen_1 = 10,
        holding_on_screen_1 = 11,
        tapped_on_screen_2 = 12,
        wait_s1_to_received_tap = 13,
        tap_correct_on_screen_2 = 14,
        t1tot2_trial_failed = 15,

        inactive_on_screen_2 = 20,
        holding_on_screen_2 = 21,
        tapped_on_screen_1 = 22,
        wait_s2_to_received_tap = 23,
        tap_correct_on_screen_1 = 24,
        t2tot1_trial_failed = 25,

        retry_status_active = 100,
    }

    public enum HoldTapResult
    {
        hold_tap_success = 0,
        hold_released_before_tap = 1,
        hold_outside_before_tap = 2,    // delay vanish
        tap_failed_to_arrive_pos = 3,   // delay vanish

        user_skip_current_trial = 100,
    }

    public enum ThrowCatchStatus
    {
        inactive_on_screen_1 = 10,
        throw_flicked_on_screen_1 = 11,
        wait_for_t1_move_phase1 = 12,
        throw_dragged_on_screen_1 = 13,
        throw_successed_on_screen_1 = 14,
        throw_failed_on_screen_1 = 15,
        wait_for_catch_on_2 = 16,
        catch_start_on_screen_2 = 17,
        t1_move_phase2_ongoing = 18,
        t1_move_phase2_acrossing_over = 19,
        catch_end_on_screen_2 = 20,
        t1tot2_trial_failed = 21,

        inactive_on_screen_2 = 30,
        throw_flicked_on_screen_2 = 31,
        wait_for_t2_move_phase1 = 32,
        throw_dragged_on_screen_2 = 33,
        throw_successed_on_screen_2 = 34,
        throw_failed_on_screen_2 = 35,
        wait_for_catch_on_1 = 36,
        catch_start_on_screen_1 = 37,
        t2_move_phase2_ongoing = 38,
        t2_move_phase2_acrossing_over = 39,
        catch_end_on_screen_1 = 40,
        t2tot1_trial_failed = 41,

        retry_status_active = 100,
    }

    public enum ThrowCatchPhase1Result
    {
        tc_throw_drag = 0,
        tc_throw_flick = 1,
    }

    public enum ThrowCatchResult
    {
        throw_catch_success = 0,
        throw_downgraded_to_drag_due_d = 1,
        throw_downgraded_to_drag_due_v = 2,
        throw_downgraded_to_drag_due_dv = 3,
        throw_to_wrong_dir = 4,
        catch_failed_to_arrive_pos = 5,     // delay vanish
        user_skip_current_trial = 100,
    }
    
    
    public struct tech1DirectDragTrialData
    {
        int trialid, firstid, secondid;
        public Vector2 device2FirstTouchPosition, device2FirstCorrectPosition;
        public long device2FirstTouchStamp, device2FirstCorrectTouchStamp;
        long timespanBetweenFirstAndCorrectTouch;

        public void init(int tid, int id1, int id2)
        {
            trialid = tid;
            firstid = id1;
            secondid = id2;
            device2FirstTouchPosition = device2FirstCorrectPosition = Vector2.zero;
            device2FirstTouchStamp = device2FirstCorrectTouchStamp = timespanBetweenFirstAndCorrectTouch = 0;
        }

        public string getAllData()
        {
            timespanBetweenFirstAndCorrectTouch = device2FirstCorrectTouchStamp - device2FirstTouchStamp;
            string str = "";
            str = trialid.ToString() + techParamSeperator
                + firstid.ToString() + techParamSeperator + secondid.ToString() + techParamSeperator
                + device2FirstTouchPosition.ToString() + techParamSeperator
                + device2FirstCorrectPosition.ToString() + techParamSeperator
                + device2FirstTouchStamp.ToString() + techParamSeperator
                + device2FirstCorrectTouchStamp.ToString() + techParamSeperator
                + timespanBetweenFirstAndCorrectTouch.ToString() + techParamSeperator
                ;
            return str;
        }
    }

    public struct tech2HoldTapTrialData
    {
        int trialid, firstid, secondid;
        public float maxOffset, minOffset;
        public Vector2 holdStartPosition, holdMaxOffsetPosition, holdMinOffsetPosition;

        public void init(int tid, int id1, int id2)
        {
            trialid = tid;
            firstid = id1;
            secondid = id2;
            maxOffset = 0f;
            minOffset = float.MaxValue;
            holdStartPosition = holdMaxOffsetPosition = holdMinOffsetPosition;
        }

        public string getAllData()
        {
            string str = "";
            str = trialid.ToString() + techParamSeperator
                + firstid.ToString() + techParamSeperator + secondid.ToString() + techParamSeperator
                + maxOffset.ToString() + techParamSeperator
                + minOffset.ToString() + techParamSeperator
                + holdStartPosition.ToString() + techParamSeperator
                + holdMaxOffsetPosition.ToString() + techParamSeperator
                + holdMinOffsetPosition.ToString() + techParamSeperator
                ;
            return str;
        }
    }

    public struct tech3ThrowCatchTrialData
    {
        int trialid, firstid, secondid;
        public float deltaTime;
        public float deltaDistance;
        public float userVelocity;

        public void init(int tid, int id1, int id2)
        {
            trialid = tid;
            firstid = id1;
            secondid = id2;
            deltaTime = 0;
            deltaDistance = userVelocity = 0f;
        }

        public string getAllData()
        {
            string str = "";
            str = trialid.ToString() + techParamSeperator
                + firstid.ToString() + techParamSeperator + secondid.ToString() + techParamSeperator
                + deltaTime.ToString() + techParamSeperator
                + deltaDistance.ToString() + techParamSeperator
                + userVelocity.ToString() + techParamSeperator
                ;
            return str;
        }
    }
    
}
