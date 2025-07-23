from django.db import models
from django.core.exceptions import ValidationError
from .constants import TeamChoices

class Competences(models.Model):
    name = models.TextField()
    description = models.TextField()
    logo = models.URLField(null=True, blank=True)

class Achievements(models.Model):
    type = models.TextField()
    description = models.TextField()
    xpGiven = models.IntegerField()
    logo = models.URLField(null=True, blank=True)

class Location(models.Model):
    latitude = models.FloatField()
    longitude = models.FloatField()
    text = models.TextField()

class Team(models.Model):
    name = models.IntegerField(
        choices=TeamChoices.choices,
    )

    logo = models.URLField(null=True, blank=True)

    def __str__(self):
        return self.get_name_display()

class PhotoBase(models.Model):
    photo_by_user = models.ForeignKey(
        'user.CustomUser', 
        related_name="user_personal_gallery",  # Allows reverse query: user.user_profile_gallery.all()
        on_delete=models.CASCADE,
        null=True,
        blank=True,
    )

    photo_by_ngo = models.ForeignKey(
        'ngo.NGOEntity',  # Replace with your NGO model
        related_name="ngo_personal_gallery",
        on_delete=models.CASCADE,
        blank=True,
        null=True
    )

    event_related_photo = models.ForeignKey(
        'events.Event',  # Replace with your NGO model
        related_name="event_gallery",
        on_delete=models.CASCADE,
        blank=True,
        null=True
    )

    large_image = models.URLField()
    thumbnail_image = models.URLField(blank=True, null=True)
    caption = models.CharField(max_length=255, blank=True)
    created_at = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        abstract = True 
    
    def __str__(self):
        return self.caption
    
    def clean(self):
        if not self.photo_by_user and not self.photo_by_ngo and not self.event_related_photo:
            raise ValidationError("A photo must belong to either a user, NGO, or an event.")
        if self.photo_by_user and self.photo_by_ngo:
            raise ValidationError("A photo cannot belong to both a user and an NGO.")

class ProfilePicture(models.Model):
    user = models.OneToOneField('user.CustomUser', on_delete=models.CASCADE, related_name='user_profile_picture', null=True, blank=True)
    ngo = models.ForeignKey('ngo.NGOEntity', on_delete=models.CASCADE, related_name='ngo_profile_pictures', null=True, blank=True)
    image = models.URLField(null=True, blank=True, max_length=500)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def save(self, *args, **kwargs):
        # Ensure either 'user' or 'ngo' is set, but not both
        if not (self.user or self.ngo):
            raise ValueError("Either user or NGO must be specified for the profile picture.")
        if self.user and self.ngo:
            raise ValueError("Cannot assign both user and NGO to the same profile picture.")
        super().save(*args, **kwargs)
    
class Photo(PhotoBase):
    pass