from datetime import timedelta
from django.utils.timezone import now
from .models import CustomUser

def reset_streaks():
    yesterday = now().date() - timedelta(days=1)

    users_to_reset = CustomUser.objects.filter(last_active_date__lt=yesterday, streak__gt=0)
    users_to_reset.update(streak=0)

    print(f"Streaks reset for {users_to_reset.count()} users.")
