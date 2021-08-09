﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicDragParams : MonoBehaviour
{
    public const float DRAG_MIN_X = -4f, DRAG_MAX_X = 4f, DRAG_MIN_Y = -10f, DRAG_MAX_Y = 10f;

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

        inactive_on_screen_2 = 20,
        drag_phase1_on_screen_2 = 21,
        across_from_screen_2 = 22,
        drag_phase1_end_on_screen_2 = 23,
        wait_for_drag_on_1 = 24,
        drag_phase2_on_screen_1 = 25,
        across_end_from_screen_2 = 26,
        drag_phase2_ongoing_on_screen_1 = 27,
        drag_phase2_end_on_screen_1 = 28,
    }

    public enum HoldTapStatus
    {
        inactive_on_screen_1 = 10,
        holding_on_screen_1 = 11,
        tapped_on_screen_2 = 12,

        inactive_on_screen_2 = 20,
        holding_on_screen_2 = 21,
        tapped_on_screen_1 = 22,
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
    }

    public enum ThrowCatchPhase1Result
    {
        tc_throw_drag = 0,
        tc_throw_flick = 1,
    }

}