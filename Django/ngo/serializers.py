from rest_framework import serializers
from django.contrib.auth import get_user_model
from django.db import transaction

from ngo.models import NGOEntity, SocialMedia, Contact
from shared.serializers import PhotoSerializer, UserReviewSerializer, SocialMediaSerializer, ContactSerializer, TeamSerializer
from shared.constants import SocialMedia as SocialMediaChoices
from shared.constants import ContactType as ContactTypeChoices
from shared.constants import NGOOfferings
from user.models import UserEventInteraction, CustomUser
from events.models import Event

User = get_user_model()

class CaseInsensitiveSocialMediaField(serializers.CharField):
    """
    A custom field that handles case-insensitive social media platform names.
    """
    def to_internal_value(self, data):
        if not isinstance(data, str):
            return super().to_internal_value(data)
        
        # Normalize input to title case for comparison
        normalized_data = data.strip().title()
        
        # Special case for YouTube (camelCase)
        if normalized_data.lower() == 'youtube':
            return 'YouTube'
            
        # Special case for TikTok (camelCase)
        if normalized_data.lower() == 'tiktok':
            return 'TikTok'
            
        # Special case for LinkedIn (camelCase)
        if normalized_data.lower() == 'linkedin':
            return 'LinkedIn'
        
        # Handle other platforms by checking against available choices
        for choice, _ in SocialMediaChoices.choices:
            if choice.lower() == normalized_data.lower():
                return choice
        
        # If no match found, return original data for normal validation
        return super().to_internal_value(data)


class CaseInsensitiveContactTypeField(serializers.CharField):
    """
    A custom field that handles case-insensitive contact type names.
    """
    def to_internal_value(self, data):
        if not isinstance(data, str):
            return super().to_internal_value(data)
        
        # Normalize input for comparison
        normalized_data = data.strip().title()
        
        # Special case for E-mail
        if normalized_data.lower() in ['email', 'e-mail', 'e mail']:
            return 'E-mail'
            
        # Special case for Phone
        if normalized_data.lower() in ['phone', 'phone number', 'telephone']:
            return 'Phone'
            
        # Handle other contact types by checking against available choices
        for choice, _ in ContactTypeChoices.choices:
            if choice.lower() == normalized_data.lower():
                return choice
        
        # If no match found, return original data for normal validation
        return super().to_internal_value(data)


class NGOProfileSerializer(serializers.ModelSerializer):

    focus_teams = serializers.SerializerMethodField()
    user_reviews = UserReviewSerializer(many=True, read_only=True)
    social_media = SocialMediaSerializer(many=True, read_only=True)
    contact_options = ContactSerializer(many=True, read_only=True)
    ngo_profile_gallery = PhotoSerializer(many=True, read_only=True, source="ngo_personal_gallery")
    profile_image_url = serializers.URLField(source='ngo_profile_pictures.first.image', required=False)

    class Meta:
        model = NGOEntity
        fields = '__all__'

    def get_focus_teams(self, obj):
        teams = obj.focus_teams.all()
        return [team.get_name_display() for team in teams]

    # def get_profile_picture(self, obj):
    #     profile_picture = obj.ngo_profile_pictures.first()
    #     if profile_picture and profile_picture.image:
    #         return profile_picture.image
    #     return None  # If no profile picture exists, return None


class SocialMediaDetailSerializer(serializers.ModelSerializer):
    platform_display = serializers.CharField(source='get_platform_display', read_only=True)
    platform = CaseInsensitiveSocialMediaField()
    
    class Meta:
        model = SocialMedia
        fields = ['id', 'platform', 'platform_display', 'url']
        read_only_fields = ['id']
        
    def validate_platform(self, value):
        """
        Check that the platform is one of the allowed choices.
        """
        # Already normalized by our custom field, just need to check if it's in choices
        valid_platforms = dict(SocialMediaChoices.choices).keys()
        if value not in valid_platforms:
            valid_platforms_list = ", ".join(valid_platforms)
            raise serializers.ValidationError(
                f"Invalid platform. Supported platforms are: {valid_platforms_list}"
            )
        return value


class ContactDetailSerializer(serializers.ModelSerializer):
    contact_type_display = serializers.CharField(source='get_contact_type_display', read_only=True)
    contact_type = CaseInsensitiveContactTypeField()
    
    class Meta:
        model = Contact
        fields = ['id', 'name', 'contact_type', 'contact_type_display', 'contact_info']
        read_only_fields = ['id']
        
    def validate_contact_type(self, value):
        """
        Check that the contact type is one of the allowed choices.
        """
        valid_types = dict(ContactTypeChoices.choices).keys()
        if value not in valid_types:
            valid_types_list = ", ".join(valid_types)
            raise serializers.ValidationError(
                f"Invalid contact type. Supported types are: {valid_types_list}"
            )
        return value


class NGOPortalProfileSerializer(serializers.ModelSerializer):
    """
    Serializer for NGO profile in the portal with detailed information and edit capabilities.
    """
    focus_teams = TeamSerializer(many=True, read_only=True)
    social_media = SocialMediaDetailSerializer(many=True, read_only=True)
    contact_options = ContactDetailSerializer(many=True, read_only=True)
    ngo_profile_gallery = PhotoSerializer(many=True, read_only=True, source="ngo_personal_gallery")
    profile_image = serializers.SerializerMethodField()
    admin_user_email = serializers.EmailField(source='admin_user.email', read_only=True)
    
    # Statistics
    event_count = serializers.SerializerMethodField()
    volunteer_count = serializers.IntegerField(source='volonteer_num', read_only=True)
    
    class Meta:
        model = NGOEntity
        fields = [
            'id', 'name', 'about', 'history', 'contact_person_bio',
            'volonteer_num', 'avg_rating', 'total_ratings', 'total_xp',
            'focus_teams', 'social_media', 'contact_options',
            'ngo_profile_gallery', 'profile_image', 'admin_user', 'admin_user_email',
            'event_count', 'volunteer_count'
        ]
        read_only_fields = ['id', 'avg_rating', 'total_ratings', 'total_xp', 'admin_user', 'volonteer_num']
    
    def get_profile_image(self, obj):
        profile_picture = obj.ngo_profile_pictures.first()
        if profile_picture and profile_picture.image:
            return profile_picture.image
        return None
    
    def get_event_count(self, obj):
        # Fix: Use the appropriate related manager name based on your Event model
        # The error shows 'event_set' doesn't exist, so we need to find the correct related name
        try:
            # Try the default related name
            from events.models import Event
            return Event.objects.filter(organized_by=obj).count()
        except ImportError:
            # If we can't import Event, return 0
            return 0
    
    def update(self, instance, validated_data):
        """
        Update the NGO profile with validated data.
        """
        for attr, value in validated_data.items():
            setattr(instance, attr, value)
        instance.save()
        return instance


class UserDetailSerializer(serializers.ModelSerializer):
    """Serializer for User details in application management"""
    full_name = serializers.SerializerMethodField()
    
    class Meta:
        model = CustomUser
        fields = ['id', 'email', 'name', 'surname', 'full_name', 'birthday', 'bio', 'total_xp']
        read_only_fields = fields
    
    def get_full_name(self, obj):
        if obj.name and obj.surname:
            return f"{obj.name} {obj.surname}"
        return obj.name or obj.email


class ApplicationListSerializer(serializers.ModelSerializer):
    """Serializer for listing event applications for NGO admin"""
    user = UserDetailSerializer(read_only=True)
    event_name = serializers.CharField(source='event.eventName', read_only=True)
    event_id = serializers.IntegerField(source='event.id', read_only=True)
    status_display = serializers.CharField(source='get_status_display', read_only=True)
    application_date = serializers.DateTimeField(source='created_at', read_only=True)
    
    class Meta:
        model = UserEventInteraction
        fields = ['id', 'user', 'event_id', 'event_name', 'status', 'status_display', 'application_date']
        read_only_fields = ['id', 'user', 'event_id', 'event_name', 'application_date']


class ApplicationUpdateSerializer(serializers.ModelSerializer):
    """Serializer for updating application status"""
    
    class Meta:
        model = UserEventInteraction
        fields = ['id', 'status']
        read_only_fields = ['id']


class NGORegistrationSerializer(serializers.Serializer):
    """
    Serializer for NGO registration that creates both the NGO and admin user.
    """
    # Organization Information
    organization_name = serializers.CharField(max_length=255)
    website = serializers.URLField(required=False, allow_blank=True)
    registration_number = serializers.CharField(max_length=100, required=False, allow_blank=True)
    
    # Contact Person Information
    contact_person_first_name = serializers.CharField(max_length=100)
    contact_person_last_name = serializers.CharField(max_length=100)
    contact_person_email = serializers.EmailField()
    contact_person_phone = serializers.CharField(max_length=20, required=False, allow_blank=True)
    
    # Admin Account Information
    password = serializers.CharField(write_only=True, min_length=8)
    confirm_password = serializers.CharField(write_only=True)
    
    # Organization Details
    operating_countries = serializers.CharField(required=False, allow_blank=True)
    organization_offerings = serializers.MultipleChoiceField(
        choices=NGOOfferings.choices,
        required=False,
        allow_empty=True
    )
    
    def validate(self, data):
        """
        Validate the registration data.
        """
        # Check password confirmation
        if data['password'] != data['confirm_password']:
            raise serializers.ValidationError("Passwords do not match.")
        
        # Check if email already exists
        if User.objects.filter(email=data['contact_person_email']).exists():
            raise serializers.ValidationError("A user with this email already exists.")
        
        # Check if organization name already exists (case-insensitive)
        if NGOEntity.objects.filter(name__iexact=data['organization_name']).exists():
            raise serializers.ValidationError("An organization with this name already exists.")
        
        return data
    
    @transaction.atomic
    def create(self, validated_data):
        """
        Create the NGO and admin user in a single transaction.
        """
        # Extract admin user data
        contact_first_name = validated_data['contact_person_first_name']
        contact_last_name = validated_data['contact_person_last_name']
        contact_email = validated_data['contact_person_email']
        contact_phone = validated_data.get('contact_person_phone', '')
        password = validated_data['password']
        
        # Create admin user
        admin_user = User.objects.create_user(
            email=contact_email,
            name=contact_first_name,
            surname=contact_last_name,
            password=password,
            is_staff=True  # Give them staff permissions for NGO admin
        )
        
        # Prepare NGO data
        ngo_data = {
            'name': validated_data['organization_name'],
            'website': validated_data.get('website', ''),
            'registration_number': validated_data.get('registration_number', ''),
            'contact_person_name': f"{contact_first_name} {contact_last_name}",
            'contact_person_email': contact_email,
            'contact_person_phone': contact_phone,
            'operating_countries': validated_data.get('operating_countries', ''),
            'admin_user': admin_user
        }
        
        # Handle offerings (convert list to comma-separated string)
        offerings = validated_data.get('organization_offerings', [])
        if offerings:
            ngo_data['offerings'] = ','.join(offerings)
        
        # Create NGO
        ngo = NGOEntity.objects.create(**ngo_data)
        
        # Create default contact entry for the contact person
        if contact_email:
            Contact.objects.create(
                ngo=ngo,
                name=f"{contact_first_name} {contact_last_name}",
                contact_type='E-mail',
                contact_info=contact_email
            )
        
        if contact_phone:
            Contact.objects.create(
                ngo=ngo,
                name=f"{contact_first_name} {contact_last_name}",
                contact_type='Phone',
                contact_info=contact_phone
            )
        
        return {
            'ngo': ngo,
            'admin_user': admin_user,
            'message': 'NGO registration successful'
        }
    
    def to_representation(self, instance):
        """
        Customize the response data.
        """
        if isinstance(instance, dict) and 'ngo' in instance:
            ngo = instance['ngo']
            admin_user = instance['admin_user']
            
            return {
                'id': ngo.id,
                'organization_name': ngo.name,
                'contact_person_email': admin_user.email,
                'admin_user_id': admin_user.id,
                'registration_date': ngo.created_at.isoformat() if ngo.created_at else None,
                'message': instance.get('message', 'Registration successful')
            }
        
        return super().to_representation(instance)