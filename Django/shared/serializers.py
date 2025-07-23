from rest_framework import serializers

from shared.models import Achievements, Competences, Photo, Team
from shared.constants import SocialMedia
from ngo.models import Contact, SocialMedia
from user.models import UserReview

class TeamSerializer(serializers.ModelSerializer):

    name = serializers.CharField(source='get_name_display') 

    class Meta:
        model = Team
        fields = '__all__'

class PhotoSerializer(serializers.ModelSerializer):
     
    class Meta:
        model = Photo
        fields = ['id', 'thumbnail_image', 'large_image', 'caption']

class AchievementsSerializer(serializers.ModelSerializer):
    class Meta:
        model = Achievements
        fields = ["type", "description", "xpGiven"]

class CompetencesSerializer(serializers.ModelSerializer):
    class Meta:
        model = Competences
        fields = '__all__'

class UserReviewSerializer(serializers.ModelSerializer):
    reviewer_name = serializers.SerializerMethodField()
    reviewer_photo = serializers.ReadOnlyField(source="user_reviewer.profile_image_url")

    class Meta:
        model = UserReview
        fields = ["reviewer_name", "review_comment", "review_rating", "date_reviewed", "reviewer_photo"]

    def get_reviewer_name(self, obj):
        user = obj.user_reviewer
        return f"{user.name} {user.surname}"

class SocialMediaSerializer(serializers.ModelSerializer):
    class Meta:
        model = SocialMedia
        fields = ['platform', 'url']

class ContactSerializer(serializers.ModelSerializer):
    class Meta:
        model = Contact
        fields = ['ngo', 'contact_type', 'contact_info']
