from user.models import UserEventInteraction, UserSavedEvent
from .utils import get_starting_in
from rest_framework import serializers
from shared.models import Team, Competences
from .models import Event, Location, TRRDonation
from shared.utils import format_datetime, parse_datetime
from ngo.serializers import NGOProfileSerializer
from django.apps import apps
from rest_framework import generics
from shared.serializers import PhotoSerializer, AchievementsSerializer, CompetencesSerializer
from django.utils.timezone import make_aware
from datetime import datetime

# Get NGOEntity model
NGOEntity = apps.get_model('ngo', 'NGOEntity')

class TeamSerializer(serializers.ModelSerializer):
    name = serializers.CharField(source='get_name_display') 

    class Meta:
        model = Team
        fields = ['id', 'name']

class LocationSerializer(serializers.ModelSerializer):
    class Meta:
        model = Location
        fields = '__all__'

class MapMarkerSerializer(serializers.ModelSerializer):
    location = LocationSerializer()
    focus_teams = serializers.SerializerMethodField()
    starting_in = serializers.SerializerMethodField()

    class Meta:
        model = Event
        fields = ['id', 'eventName', 'markerType', 'eventDescription', 
                                 'location', 'main_image_url', 'focus_teams', 'starting_in', 'people_needed', 'people_applied']

    def get_focus_teams(self, obj):
        teams = obj.focus_teams.all()
        return [team.get_name_display() for team in teams]
    
    def get_starting_in(self, obj):
        return get_starting_in(obj.startDate, obj.endDate)

class CompetencesSerializer(serializers.ModelSerializer):
    class Meta:
        model = Competences
        fields = ['id', 'name', 'description']

class TRRDonationSerializer(serializers.ModelSerializer):
    class Meta:
        model = TRRDonation
        fields = ['trr_number', 'bic', 'reference', 
                  'recipient_name', 'recipient_address', 'purpose', 'purpose_code']

class CompetencesField(serializers.Field):
    """
    Custom field to handle competences that can be submitted as a list of names
    but will be handled as competence objects
    """
    def to_representation(self, value):
        # For reading, use the standard CompetencesSerializer
        return CompetencesSerializer(value.all(), many=True).data
    
    def to_internal_value(self, data):
        # For writing, accept a list of names and return a list of competence objects
        if not data or not isinstance(data, list):
            return []
        
        competence_objects = []
        for item in data:
            try:
                if isinstance(item, int) or (isinstance(item, str) and item.isdigit()):
                    # If it's already an ID, use it directly
                    comp_id = int(item)
                    competence = Competences.objects.get(id=comp_id)
                else:
                    # Otherwise look up by name
                    competence = Competences.objects.get(name=item)
                competence_objects.append(competence)
            except Competences.DoesNotExist:
                pass
        
        return competence_objects

class EventSerializer(serializers.ModelSerializer):
    startDate = serializers.DateTimeField(required=False)
    endDate = serializers.DateTimeField(required=False)
    main_image_url = serializers.CharField(required=False, allow_blank=True, allow_null=True)
    
    location = LocationSerializer(required=False)
    event_gallery = PhotoSerializer(many=True, read_only=True)
    focus_teams = serializers.SerializerMethodField(read_only=True)
    
    achievements = AchievementsSerializer(many=True, required=False, read_only=True)
    competences = CompetencesSerializer(many=True, read_only=True)
    organized_by = NGOProfileSerializer(required=False, read_only=True)
    status = serializers.SerializerMethodField(read_only=True)
    starting_in = serializers.SerializerMethodField(read_only=True)
    is_saved = serializers.SerializerMethodField(read_only=True)
    donation = TRRDonationSerializer(required=False)
    
    # Write-only fields for handling relationships
    competences_data = CompetencesField(write_only=True, required=False)
    focus_teams_data = serializers.PrimaryKeyRelatedField(
        many=True, 
        queryset=Team.objects.all(),
        write_only=True,
        required=False
    )
    organized_by_id = serializers.PrimaryKeyRelatedField(
        queryset=NGOEntity.objects.all(), 
        source='organized_by',
        write_only=True,
        required=False,
        allow_null=True
    )

    class Meta:
        model = Event
        fields = [
            'id', 'eventName', 'markerType', 'eventDescription', 'location',
            'startDate', 'endDate', 'people_needed', 'people_applied', 'event_xp', 'main_image_url',
            'event_gallery', 'focus_teams', 'achievements', 'competences',
            'organized_by', 'organized_by_id', 'status', 'starting_in', 'is_saved', 
            'donation', 'competences_data', 'focus_teams_data'
        ]

    def get_focus_teams(self, obj):
        teams = obj.focus_teams.all()
        return [team.get_name_display() for team in teams]

    def get_starting_in(self, obj):
        return get_starting_in(obj.startDate, obj.endDate)

    def get_is_saved(self, obj):
        user = self.context.get("user", None)
        if user:
            return UserSavedEvent.objects.filter(user=user, event=obj).exists()
        return None  # remove this field dynamically if there is no user in the context

    def get_status(self, obj):
        user = self.context.get("user", None)
        if user:
            interaction = UserEventInteraction.objects.filter(user=user, event=obj).first()
            return interaction.status if interaction else None
    
    def validate_startDate(self, value):
        try:
            return parse_datetime(value)
        except Exception as e:
            raise serializers.ValidationError(f"Invalid date format: {e}")
    
    def validate_endDate(self, value):
        try:
            return parse_datetime(value)
        except Exception as e:
            raise serializers.ValidationError(f"Invalid date format: {e}")
    
    def create(self, validated_data):
        # Extract nested data
        location_data = validated_data.pop('location', None)
        competences_data = validated_data.pop('competences_data', [])
        focus_teams_data = validated_data.pop('focus_teams_data', [])
        organized_by = validated_data.get('organized_by', None)
        
        # If competences_data not in validated_data but in initial_data, try to use it
        if not competences_data and 'competences' in self.initial_data:
            competences_list = self.initial_data.get('competences', [])
            if competences_list:
                competence_objects = []
                for name in competences_list:
                    try:
                        competence = Competences.objects.get(name=name)
                        competence_objects.append(competence)
                    except Competences.DoesNotExist:
                        pass
                competences_data = competence_objects
        
        # If focus_teams_data not in validated_data but in initial_data, try to use it
        if not focus_teams_data and 'focus_teams' in self.initial_data:
            focus_teams_list = self.initial_data.get('focus_teams', [])
            focus_teams_data = Team.objects.filter(id__in=focus_teams_list)
        
        # If organized_by is not in validated_data but in initial_data, try to use it
        if not organized_by and 'organized_by' in self.initial_data:
            organized_by_id = self.initial_data.get('organized_by')
            if organized_by_id:
                try:
                    organized_by = NGOEntity.objects.get(id=organized_by_id)
                    validated_data['organized_by'] = organized_by
                except NGOEntity.DoesNotExist:
                    pass
        
        # Create location if provided
        location = None
        if location_data:
            location = Location.objects.create(**location_data)
        
        # Create event
        event = Event.objects.create(
            location=location,
            **validated_data
        )
        
        # Set many-to-many relationships
        if focus_teams_data:
            event.focus_teams.set(focus_teams_data)
        
        if competences_data:
            event.competences.set(competences_data)
        
        return event
    
    def update(self, instance, validated_data):
        # Extract nested data
        location_data = validated_data.pop('location', None)
        competences_data = validated_data.pop('competences_data', None)
        focus_teams_data = validated_data.pop('focus_teams_data', None)
        organized_by = validated_data.get('organized_by', None)
        
        # If competences_data not in validated_data but in initial_data, try to use it
        if competences_data is None and 'competences' in self.initial_data:
            competences_list = self.initial_data.get('competences', [])
            if competences_list:
                competence_objects = []
                for name in competences_list:
                    try:
                        competence = Competences.objects.get(name=name)
                        competence_objects.append(competence)
                    except Competences.DoesNotExist:
                        pass
                competences_data = competence_objects
        
        # If focus_teams_data not in validated_data but in initial_data, try to use it
        if focus_teams_data is None and 'focus_teams' in self.initial_data:
            focus_teams_list = self.initial_data.get('focus_teams', [])
            focus_teams_data = Team.objects.filter(id__in=focus_teams_list)
        
        # If organized_by is not in validated_data but in initial_data, try to use it
        if not organized_by and 'organized_by' in self.initial_data:
            organized_by_id = self.initial_data.get('organized_by')
            if organized_by_id:
                try:
                    organized_by = NGOEntity.objects.get(id=organized_by_id)
                    validated_data['organized_by'] = organized_by
                except NGOEntity.DoesNotExist:
                    pass
        
        # Update location if provided
        if location_data:
            if instance.location:
                # Update existing location
                for key, value in location_data.items():
                    setattr(instance.location, key, value)
                instance.location.save()
            else:
                # Create new location
                instance.location = Location.objects.create(**location_data)
        
        # Update event fields
        for key, value in validated_data.items():
            setattr(instance, key, value)
        
        # Update many-to-many relationships
        if focus_teams_data is not None:
            instance.focus_teams.set(focus_teams_data)
        
        if competences_data is not None:
            instance.competences.set(competences_data)
        
        instance.save()
        return instance
    
    def to_representation(self, instance):
        data = super().to_representation(instance)
        
        # Format dates
        if instance.startDate:
            data['startDate'] = format_datetime(instance.startDate)
        
        if instance.endDate:
            data['endDate'] = format_datetime(instance.endDate)
        
        # Format focus_teams consistently
        # Should be a list of team names
        if 'focus_teams' not in data or not data['focus_teams']:
            teams = instance.focus_teams.all()
            data['focus_teams'] = [team.get_name_display() for team in teams]
        
        # Ensure competences are properly formatted
        if 'competences' not in data or not data['competences']:
            competences = instance.competences.all()
            data['competences'] = CompetencesSerializer(competences, many=True).data
        
        # Ensure organized_by is properly populated
        if instance.organized_by and ('organized_by' not in data or data['organized_by'] is None):
            data['organized_by'] = NGOProfileSerializer(instance.organized_by).data
        
        # Remove user-specific fields if no user in context
        if self.context.get("user") is None:
            data.pop("is_saved", None)
            data.pop("status", None)
        
        return data

class EventListView(generics.ListAPIView):
    queryset = Event.objects.all()
    serializer_class = MapMarkerSerializer

    def get_queryset(self):
        queryset = super().get_queryset()
        
        team_ids = self.request.query_params.get('team_ids', None)

        if team_ids:
            team_ids_list = []
            for team_id in team_ids.split(','):
                try:
                    team_ids_list.append(int(team_id))
                except ValueError:
                    continue  

            if team_ids_list:
                queryset = queryset.filter(focus_teams__id__in=team_ids_list).distinct()
            else:
                from rest_framework.response import Response
                from rest_framework import status
                return Response({"error": "Invalid team_ids provided"}, status=status.HTTP_400_BAD_REQUEST)

        return queryset

class UserEventsCalendar(serializers.ModelSerializer):
    startDate = serializers.SerializerMethodField()
    endDate = serializers.SerializerMethodField()

    location = LocationSerializer()
    event_gallery = PhotoSerializer(many=True) 
    achievements = AchievementsSerializer(many=True)
    competences = CompetencesSerializer(many=True, read_only=True)
    organized_by = NGOProfileSerializer()

    is_enrolled = serializers.SerializerMethodField()
    is_applied = serializers.SerializerMethodField()
    is_saved = serializers.SerializerMethodField()

    class Meta:
        model = Event
        fields = [
            'id', 'eventName', 'markerType', 'eventDescription', 
            'location', 'startDate', 'endDate', 'people_needed', 'people_applied',
            'event_xp', 'main_image_url', 'event_gallery', 'achievements',
            'competences', 'organized_by', 'is_enrolled', 'is_applied', 'is_saved'
        ]

    def get_startDate(self, instance):
        return format_datetime(instance.startDate)

    def get_endDate(self, instance):
        return format_datetime(instance.endDate)

    def get_is_enrolled(self, obj):
        user = self.context.get('user')
        return user.enrolled_events.filter(id=obj.id).exists()

    def get_is_applied(self, obj):
        user = self.context.get('user')
        return user.applied_events.filter(id=obj.id).exists()

    def get_is_saved(self, obj):
        user = self.context.get('user')
        return user.saved_events.filter(id=obj.id).exists()
