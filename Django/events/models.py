from django.db import models
from shared.models import Competences, Location, Team, Achievements
from shared.constants import MarkerType

class TRRDonation(models.Model):
    trr_number = models.CharField(max_length=50, blank=True, null=True)
    bic = models.CharField(max_length=50, blank=True, null=True)
    reference = models.CharField(max_length=50, blank=True, null=True)
    recipient_name = models.CharField(max_length=50, blank=True, null=True)
    recipient_address = models.CharField(max_length=100, blank=True, null=True)
    purpose = models.CharField(max_length=50, blank=True, null=True)
    purpose_code = models.CharField(max_length=50, blank=True, null=True)

class Event(models.Model):

    updated = models.DateTimeField(auto_now=True)
    created = models.DateTimeField(auto_now_add=True)
    startDate = models.DateTimeField(null=True, blank=True)
    endDate = models.DateTimeField(null=True, blank=True)
    eventName = models.TextField()
    markerType = models.IntegerField(choices=MarkerType.choices) 
    eventDescription = models.TextField() 
    location = models.ForeignKey(Location, on_delete=models.SET_NULL, null=True, blank=True, related_name='event_locations')
    competences = models.ManyToManyField(Competences, related_name="event_comp", null=True, blank=True)
    achievements = models.ManyToManyField(Achievements, related_name="event_achv", null=True, blank=True)
    focus_teams = models.ManyToManyField(Team, related_name='event_teams_focus', null=True, blank=True)
    people_needed = models.PositiveIntegerField(default=0)
    people_applied = models.PositiveIntegerField(default=0)
    main_image_url = models.URLField(null=True, blank=True)
    event_xp = models.IntegerField(null=True, blank=True, default=0)
    organized_by = models.ForeignKey('ngo.NGOEntity', related_name="event_organizer", on_delete=models.CASCADE, blank=True, null=True)
    donation = models.OneToOneField(TRRDonation, on_delete=models.CASCADE, null=True, blank=True)

    def save(self, *args, **kwargs):
        if self.markerType != MarkerType.Donation and self.donation:
            raise ValueError("Only events with marker type Donation can be linked to one.")
        super().save(*args, **kwargs)

    class Meta:
        ordering = ['-updated']