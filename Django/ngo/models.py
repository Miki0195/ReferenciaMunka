from django.db import models
from shared.constants import SocialMedia, ContactType, NGOOfferings

class NGOEntity(models.Model):
    # Basic Information
    name = models.TextField()
    website = models.URLField(blank=True, null=True)
    registration_number = models.CharField(max_length=100, blank=True, null=True)
    
    # Contact Person Information
    contact_person_name = models.CharField(max_length=200, blank=True, null=True)
    contact_person_email = models.EmailField(blank=True, null=True)
    contact_person_phone = models.CharField(max_length=20, blank=True, null=True)
    contact_person_bio = models.TextField(blank=True, null=True)
    
    # Organization Details
    about = models.TextField(null=True, blank=True)
    history = models.TextField(null=True, blank=True)
    operating_countries = models.TextField(blank=True, null=True, help_text="Countries where the organization operates (comma-separated)")
    
    # What the organization offers (multiple choices stored as comma-separated values)
    offerings = models.CharField(max_length=200, blank=True, null=True, help_text="Comma-separated list of offerings")
    
    # Statistics and Ratings
    volonteer_num = models.IntegerField(default=0)
    avg_rating = models.FloatField(default=0)
    total_ratings = models.PositiveIntegerField(default=0)
    total_xp = models.IntegerField(default=0)
    
    # Relationships
    ngo_profile_gallery = models.ManyToManyField('shared.Photo', related_name='ngo_gallery', blank=True)
    focus_teams = models.ManyToManyField('shared.Team', related_name='ngo_focus_teams', null=True, blank=True)
    admin_user = models.ForeignKey('user.CustomUser', on_delete=models.SET_NULL, related_name='managed_ngo', null=True, blank=True)
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return self.name
    
    def get_offerings_list(self):
        """Return offerings as a list"""
        if self.offerings:
            return [offering.strip() for offering in self.offerings.split(',')]
        return []
    
    def set_offerings_list(self, offerings_list):
        """Set offerings from a list"""
        self.offerings = ','.join(offerings_list) if offerings_list else ''

class SocialMedia(models.Model):
    ngo = models.ForeignKey(NGOEntity, related_name="social_media", on_delete=models.CASCADE, null=True, blank=True)
    user = models.ForeignKey('user.CustomUser', related_name="user_social_media", on_delete=models.CASCADE, null=True, blank=True)
    platform = models.CharField(max_length=50, choices=SocialMedia.choices)
    url = models.URLField()

class Contact(models.Model):
    ngo = models.ForeignKey(NGOEntity, related_name="contact_options", on_delete=models.CASCADE, null=True, blank=True)
    user = models.ForeignKey('user.CustomUser', related_name="user_contact_options", on_delete=models.CASCADE, null=True, blank=True)
    name = models.CharField(max_length=100, blank=True, null=True)  
    contact_type = models.CharField(max_length=20, choices=ContactType.choices)
    contact_info = models.CharField(max_length=100)
