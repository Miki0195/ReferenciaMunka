from datetime import timedelta
from django.utils.timezone import now
from datetime import timedelta
from django.utils.timezone import now

def get_starting_in(startDate, endDate):
    current_time = now()

    if not startDate:
        return None

    time_difference = startDate - current_time

    if time_difference <= timedelta(0):
        if endDate and endDate > current_time:
            return "happening right now"
        else:
            return "past event"

    if time_difference <= timedelta(hours=1):
        return "starting soon"

    # Extract days, weeks, months
    days = time_difference.days
    weeks = days // 7
    months = days // 30

    if months > 0:
        return f"in {months} month{'s' if months > 1 else ''}"
    elif weeks > 0:
        return f"in {weeks} week{'s' if weeks > 1 else ''}"
    elif days > 0:
        return f"in {days} day{'s' if days > 1 else ''}"
    else:
        hours = time_difference.seconds // 3600
        if hours > 0:
            return f"in {hours} hour{'s' if hours > 1 else ''}"
        else:
            return "starting soon"