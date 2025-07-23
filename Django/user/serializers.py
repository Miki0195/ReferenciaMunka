from datetime import date
from rest_framework import serializers
from shared import constants
from shared.serializers import ContactSerializer, PhotoSerializer, SocialMediaSerializer
from shared.models import Location
from user import utils
from .models import Goal, CustomUser, GoalStage, Petition, Post, UserGoal, Week, WeeklyUserStats, ExperienceDevelopmentStage
from rest_framework.serializers import ModelSerializer
from shared.serializers import AchievementsSerializer, CompetencesSerializer
from django.db.models import QuerySet
from rest_framework_simplejwt.serializers import TokenObtainPairSerializer
from shared.constants import STAGE_LEVELS, DEVELOPMENT_STAGE

class CustomTokenObtainPairSerializer(TokenObtainPairSerializer):
    def validate(self, attrs):
        data = super().validate(attrs)

        data["user_id"] = self.user.id

        return data

class LocationSerializer(serializers.ModelSerializer):
    class Meta:
        model = Location
        fields = '__all__'

class UserSerializer(serializers.ModelSerializer):

    class Meta:
        model = CustomUser
        fields = ["id", "email", "name", "password"]

    def create(self, validated_data):
        user = CustomUser.objects.create(email=validated_data['email'], name=validated_data['name'] )
        user.set_password(validated_data['password'])
        user.save()
        return user

class UserPostSerializer(ModelSerializer):
    class Meta:
        model = CustomUser
        fields = ('name', 'surname')

class PostSerializer(ModelSerializer):
    created_by_user = UserPostSerializer()
    class Meta:
        model = Post()
        fields = '__all__'

class UserStatsSerializer(serializers.Serializer):
    xp_earned = serializers.IntegerField()
    rank = serializers.IntegerField()

class UserXPSerializer(serializers.Serializer):
    total_xp = serializers.IntegerField()
    number_top_finishes = serializers.IntegerField()

class UserStreakSerializer(serializers.Serializer):
    current_streak = serializers.IntegerField()

class FriendsAppliedSerializer(ModelSerializer):
    class Meta:
        model = CustomUser
        fields = ["id", "name", "surname", "profile_image_url"]

class PetitionSerializer(ModelSerializer):
    is_saved = serializers.SerializerMethodField()
    impact_gallery = PhotoSerializer()

    class Meta:
        model = Petition 
        fields = ["id", "is_saved", "title","avg_rating","xp_given","start_date","about","friends_that_signed","image_url","about_creator",
    "total_signs","created_by","impact_gallery"]
      
    def get_is_saved(self, obj):
        user = self.context.get('user')
        return user.saved_user_petitions.filter(id=obj.id).exists()

class GoalSerializer(ModelSerializer):
    class Meta:
        model = Goal
        fields = ['name', 'description', 'xp_required', 'category']

class UserGoalSerializer(ModelSerializer):
    goal = GoalSerializer()
    current_stage = serializers.SerializerMethodField()
    next_stage = serializers.SerializerMethodField()
    current_stage_progress = serializers.SerializerMethodField()

    class Meta:
        model = UserGoal
        fields = ['goal', 'xp_accumulated', 'level', 'current_stage', 'next_stage', 'current_stage_progress']

    def get_current_stage(self, obj):
        if obj.current_stage:
            return STAGE_LEVELS.get(obj.current_stage.stage_level, "Unknown")

    def get_next_stage(self, obj):
        if obj.current_stage:
            next_stage = obj.current_stage.stage_level + 1
            return STAGE_LEVELS.get(next_stage, "Unknown")

    def get_current_stage_progress(self, obj):
        xp_prev_stage = GoalStage.objects.filter(
            stage_level=(obj.current_stage.stage_level - 1)
            ).first()
        
        previous_stage_max_xp = xp_prev_stage.max_xp if xp_prev_stage else 0 

        return utils.calculate_stage_progress(
        obj.user.total_xp, obj.current_stage.min_xp, obj.current_stage.max_xp, previous_stage_max_xp
        )

class ExperienceDevelopmentStageSerializer(serializers.ModelSerializer):

    class Meta:
        model = ExperienceDevelopmentStage
        fields = ['name', 'next_stage']

class CustomUserSerializer(serializers.ModelSerializer):
    following = serializers.SerializerMethodField()
    followers = serializers.SerializerMethodField()
    gained_competences = CompetencesSerializer(many=True, read_only=True)
    gained_achievements = AchievementsSerializer(many=True, read_only=True)
    event_gallery = PhotoSerializer(many=True, read_only=True)
    created_events = serializers.SerializerMethodField()
    focus_teams = serializers.SerializerMethodField()
    
    user_photos = PhotoSerializer(many=True, read_only=True, source="user_personal_gallery")
    current_streak = serializers.IntegerField(source="userstreak.current_streak", read_only=True)
    current_league = serializers.SerializerMethodField()
    current_xp_stage = serializers.SerializerMethodField()
    next_xp_stage = serializers.SerializerMethodField()
    progress = serializers.SerializerMethodField()
    social_media = SocialMediaSerializer(many=True, read_only=True, source="user_social_media")
    contact_options = ContactSerializer(many=True, read_only=True, source="user_contact_options")
    profile_image_url = serializers.URLField(source='user_profile_picture.image', required=False)

    class Meta:
        model = CustomUser
        fields = [
            "email", "name", "surname", "total_xp", "profile_image_url", "user_photos",
            "bio", "birthday", "number_top_finishes", "following", "focus_teams",
            "followers", "gained_competences", "gained_achievements", "event_gallery", "created_events",
            "current_streak", "current_league", "causes", "expectations", "current_xp_stage", "next_xp_stage", "social_media", 
            "contact_options", "progress"
        ]
    
    def get_current_league(self, obj):
        today = date.today()
        
        # Get the current active week
        current_week = Week.objects.filter(start__lte=today, end__gte=today).first()
        if not current_week:
            return None

        # Find the user's stats for the current week
        user_stats = WeeklyUserStats.objects.filter(user=obj, week=current_week).first()
        if not user_stats:
            return None

        return current_week.league  # Return the league of the current week

    def get_following(self, obj):
        # Return the count of following users
        return obj.following.count()

    def get_followers(self, obj):
        # Return the count of followers
        return obj.followers.count()

    def get_created_events(self, obj):
        from events.serializers import EventSerializer
        events = obj.created_events.all() if isinstance(obj.created_events, QuerySet) else [obj.created_events]
        events = [event for event in events if event is not None]
        return EventSerializer(events, many=True).data if events else []

    def get_focus_teams(self, obj):
        teams = obj.focus_teams.all()
        return [team.get_name_display() for team in teams]

    def get_current_xp_stage(self, obj):
        if obj.current_stage:
            return DEVELOPMENT_STAGE.get(obj.current_stage.stage_level, "Unknown")

    def get_next_xp_stage(self, obj):
        if obj.current_stage:
            next_stage = obj.current_stage.stage_level + 1
            return DEVELOPMENT_STAGE.get(next_stage, "Unknown")

    def get_progress(self, obj):
        xp_prev_stage = ExperienceDevelopmentStage.objects.filter(
            stage_level=obj.current_stage.stage_level - 1
            ).first()
    
        previous_stage_max_xp = xp_prev_stage.max_xp if xp_prev_stage else 0 

        return utils.calculate_stage_progress(
        obj.total_xp, obj.current_stage.min_xp, obj.current_stage.max_xp, previous_stage_max_xp
        )

class UserEditSerializer(serializers.ModelSerializer):

    birthday = serializers.DateField(input_formats=['%d/%m/%Y', '%Y-%m-%d'])

    class Meta:
        model = CustomUser
        fields = ['email', 'name', 'surname', 'total_xp', 
                  'birthday', 'bio', 'number_top_finishes','focus_teams', 'causes', 'expectations']