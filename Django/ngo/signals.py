from django.db.models.signals import post_save, pre_save
from django.dispatch import receiver
from user.models import UserEventInteraction
from shared.constants import EventEnrollementStatus
import logging

# Set up logger for debugging
logger = logging.getLogger(__name__)

@receiver(post_save, sender=UserEventInteraction)
def update_event_volunteer_count(sender, instance, created, **kwargs):
    """
    Update the event's people_applied count when an application status changes.
    Count only applications with status 'accepted'.
    """
    event = instance.event
    
    # Count all accepted applications for this event
    accepted_count = UserEventInteraction.objects.filter(
        event=event,
        status=EventEnrollementStatus.ACCEPTED
    ).count()
    
    # Update the event's people_applied count
    if event.people_applied != accepted_count:
        logger.info(f"Updating event {event.id} volunteers count from {event.people_applied} to {accepted_count}")
        event.people_applied = accepted_count
        event.save(update_fields=['people_applied'])
