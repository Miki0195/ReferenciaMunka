def format_datetime(datetime_obj):
    if datetime_obj:
        if isinstance(datetime_obj, str):
            try:
                from django.utils.timezone import make_aware
                from datetime import datetime
                dt = make_aware(datetime.fromisoformat(datetime_obj.replace('Z', '+00:00')))
                return dt.strftime("%d-%b-%Y %H:%M")
            except (ValueError, TypeError):
                return datetime_obj
        else:
            return datetime_obj.strftime("%d-%b-%Y %H:%M")
    return None

def parse_datetime(datetime_value):
    """
    Parse a string or datetime object into a timezone-aware datetime object.
    Returns None if the input is None, or raises ValueError for invalid formats.
    """
    if datetime_value is None:
        return None
        
    if isinstance(datetime_value, str):
        from django.utils.timezone import make_aware
        from datetime import datetime
        dt = datetime.fromisoformat(datetime_value.replace('Z', '+00:00'))
        if dt.tzinfo is None:
            dt = make_aware(dt)
        return dt
    
    # If it's already a datetime object, return it
    return datetime_value