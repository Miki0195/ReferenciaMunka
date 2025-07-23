from django.utils import timezone
from django.db.models.signals import post_save, post_delete
from django.db import models
from django.dispatch import receiver
from .models import WeeklyUserStats, UserEventInteraction, CustomUser, Goal, UserGoal, ExperienceDevelopmentStage, UserStreak
from django.core.mail import send_mail
from django.template.loader import render_to_string
from django.conf import settings
import logging

# Set up logger for email errors
logger = logging.getLogger(__name__)

@receiver(post_delete, sender=WeeklyUserStats)
def update_total_xp_on_delete(sender, instance, **kwargs):
    instance.user.total_xp = WeeklyUserStats.objects.filter(user=instance.user).aggregate(models.Sum('xp_earned'))['xp_earned__sum'] or 0
    instance.user.save()

@receiver(post_save, sender=UserEventInteraction)
def send_email_on_status_change(sender, instance, **kwargs):
    # TEMPORARILY DISABLED FOR TESTING
    # Early return to skip all email sending
    return
    
    # Original email sending code below
    # if instance.status == 'applied':
    #     subject = f'New Application for Event: {instance.event.eventName}'
    #     message = render_to_string('user/emails/new_application_email.txt', {
    #         'event': instance.event,
    #         'user': instance.user,
    #     })
    #     send_mail(
    #         subject,
    #         message,
    #         settings.EMAIL_HOST_USER,
    #         ['info@greenie.si'],
    #         fail_silently=False,
    #     )

    if instance.status == 'accepted':
        # Get email settings from Django settings
        sender_email = settings.EMAIL_HOST_USER

        # Check if email settings are configured
        if not sender_email or not settings.EMAIL_HOST:
            logger.error("Email settings are not properly configured. Skipping email notification.")
            return
            
        subject = f'Your Application Has Been Accepted: {instance.event.eventName}'
        try:
            message = render_to_string('user/emails/application_accepted_email.txt', {
                'event': instance.event,
                'user': instance.user,
            })
            
            send_mail(
                subject,
                message,
                sender_email,
                [instance.user.email],
                fail_silently=True,  # Don't let email errors crash the application
            )
            logger.info(f"Acceptance email sent to {instance.user.email} via signal")
        except Exception as e:
            logger.error(f"Failed to send email via signal: {str(e)}")


@receiver(post_save, sender=CustomUser)
def assign_default_goal(sender, instance, created, **kwargs):
    if created: 
        default_goal = Goal.objects.first()
        if default_goal:
            first_stage = default_goal.stages.order_by("min_xp").first()

            UserGoal.objects.create(
                user=instance,
                goal=default_goal,
                current_stage=first_stage,
                xp_accumulated=0
            )

@receiver(post_save, sender=CustomUser)
def set_default_stage(sender, instance, created, **kwargs):
    if created and not instance.current_stage:
        default_stage = ExperienceDevelopmentStage.objects.order_by('stage_level').first()
        if default_stage:
            instance.current_stage = default_stage
            instance.save()

@receiver(post_save, sender=CustomUser)
def create_user_streak(sender, instance, created, **kwargs):
    if created:  # Only run when the user is first created
    
        UserStreak.objects.create(
            user=instance,
            last_active_date=timezone.now().date()  # Set the last_active_date to the current date
        )