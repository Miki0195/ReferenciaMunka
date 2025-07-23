# your_app/utils.py

from django.db import transaction
from django.core.exceptions import ValidationError

def add_xp_to_user(user, xp_earned):
    if xp_earned == 0:
        return
    elif xp_earned < 0:
            raise ValidationError("XP earned must be a positive value.")

    user_goal = user.usergoal_set.filter(status=1).order_by('-xp_accumulated').first()

    with transaction.atomic():
        if user_goal:
            user_goal.xp_accumulated += xp_earned
            user_goal.save(update_fields=['xp_accumulated', 'status'])
        user.total_xp += xp_earned
        user.save(update_fields=['total_xp', 'current_stage'])
        
def calculate_stage_progress(total_xp, stage_min_xp, stage_max_xp, previous_stage_max_xp):

    current_xp_in_stage = total_xp - previous_stage_max_xp

    stage_range = stage_max_xp - stage_min_xp
    progress_percentage = (current_xp_in_stage / stage_range) if stage_range else 0.0

    return round(progress_percentage, 2)
