from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from rest_framework import status
from rest_framework.permissions import IsAuthenticated
from rest_framework_simplejwt.tokens import RefreshToken

from ngo.models import NGOEntity, SocialMedia, Contact
from shared.models import Photo
from ngo.serializers import (
    NGOProfileSerializer, NGOPortalProfileSerializer, SocialMediaDetailSerializer, 
    ContactDetailSerializer, ApplicationListSerializer, ApplicationUpdateSerializer,
    NGORegistrationSerializer
)
from shared.serializers import PhotoSerializer
from user.models import CustomUser, UserEventInteraction
from shared.constants import EventEnrollementStatus
from events.models import Event 
from django.core.mail import send_mail
from django.template.loader import render_to_string
from django.conf import settings
from django.shortcuts import get_object_or_404
from django.db.models import Q
from django.utils import timezone
import logging

# Set up logger for email errors
logger = logging.getLogger(__name__)

@api_view(['POST'])
def register_ngo(request):
    """
    Register a new NGO organization with an admin user.
    
    Expected data:
    {
        "organization_name": "Example NGO",
        "website": "https://example.org",
        "registration_number": "12345",
        "contact_person_first_name": "John",
        "contact_person_last_name": "Doe",
        "contact_person_email": "john@example.org",
        "contact_person_phone": "+1234567890",
        "password": "securepassword123",
        "confirm_password": "securepassword123",
        "operating_countries": "USA, Canada",
        "organization_offerings": ["Volunteering", "Events"]
    }
    """
    serializer = NGORegistrationSerializer(data=request.data)
    
    if serializer.is_valid():
        try:
            result = serializer.save()
            return Response(
                serializer.to_representation(result),
                status=status.HTTP_201_CREATED
            )
        except Exception as e:
            logger.error(f"NGO Registration Error: {str(e)}")
            return Response(
                {"error": "Registration failed. Please try again."},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            )
    
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

@api_view(['GET'])
def getNGOProfile(request, ngo_id):
    try:
        ngo_profile = NGOEntity.objects.get(id=ngo_id)
        serializer = NGOProfileSerializer(ngo_profile)
        return Response(serializer.data, status=status.HTTP_200_OK)
    except NGOEntity.DoesNotExist:
        return Response({"error": "NGO Profile does not exist."}, status=status.HTTP_404_NOT_FOUND)

@api_view(['POST'])
def ngo_login(request):
    """
    Login endpoint specifically for NGO portal access.
    """
    data = request.data
    email = data.get('email')
    password = data.get('password')
    
    if not email or not password:
        return Response({"error": "Email and password are required."}, status=status.HTTP_400_BAD_REQUEST)
    
    try:
        user = CustomUser.objects.get(email=email)
    except CustomUser.DoesNotExist:
        return Response({"error": "Invalid credentials."}, status=status.HTTP_401_UNAUTHORIZED)
    
    if not user.check_password(password):
        return Response({"error": "Invalid credentials."}, status=status.HTTP_401_UNAUTHORIZED)
    
    try:
        ngo = NGOEntity.objects.get(admin_user=user)
    except NGOEntity.DoesNotExist:
        return Response({"error": "User is not associated with any NGO."}, status=status.HTTP_403_FORBIDDEN)
    
    refresh = RefreshToken.for_user(user)
    
    return Response({
        'refresh': str(refresh),
        'access': str(refresh.access_token),
        'user_id': user.id,
        'ngo_id': ngo.id,
        'ngo_name': ngo.name
    }, status=status.HTTP_200_OK)  


# NGO Portal Profile Management Views
@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_portal_profile(request):
    """
    Retrieve the NGO profile for the currently authenticated admin user.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Serialize the NGO profile
        serializer = NGOPortalProfileSerializer(ngo)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['PUT', 'PATCH'])
@permission_classes([IsAuthenticated])
def update_ngo_portal_profile(request):
    """
    Update the NGO profile for the currently authenticated admin user.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Partial update (PATCH) or full update (PUT)
        partial = request.method == 'PATCH'
        serializer = NGOPortalProfileSerializer(ngo, data=request.data, partial=partial)
        
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_200_OK)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_social_media(request):
    """
    Retrieve all social media entries for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        social_media = SocialMedia.objects.filter(ngo=ngo)
        serializer = SocialMediaDetailSerializer(social_media, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['POST'])
@permission_classes([IsAuthenticated])
def create_ngo_social_media(request):
    """
    Create a new social media entry for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Add the NGO to the data
        data = request.data.copy()
        data['ngo'] = ngo.id
        
        serializer = SocialMediaDetailSerializer(data=data)
        if serializer.is_valid():
            serializer.save(ngo=ngo)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['PUT', 'DELETE'])
@permission_classes([IsAuthenticated])
def update_delete_ngo_social_media(request, pk):
    """
    Update or delete a specific social media entry for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        try:
            social_media = SocialMedia.objects.get(id=pk, ngo=ngo)
        except SocialMedia.DoesNotExist:
            return Response(
                {"error": "Social media not found or not associated with this NGO."},
                status=status.HTTP_404_NOT_FOUND
            )
        
        if request.method == 'DELETE':
            social_media.delete()
            return Response(status=status.HTTP_204_NO_CONTENT)
        
        # For PUT
        serializer = SocialMediaDetailSerializer(social_media, data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_200_OK)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_contacts(request):
    """
    Retrieve all contact entries for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        contacts = Contact.objects.filter(ngo=ngo)
        serializer = ContactDetailSerializer(contacts, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['POST'])
@permission_classes([IsAuthenticated])
def create_ngo_contact(request):
    """
    Create a new contact entry for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Add the NGO to the data
        data = request.data.copy()
        data['ngo'] = ngo.id
        
        serializer = ContactDetailSerializer(data=data)
        if serializer.is_valid():
            serializer.save(ngo=ngo)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['PUT', 'DELETE'])
@permission_classes([IsAuthenticated])
def update_delete_ngo_contact(request, pk):
    """
    Update or delete a specific contact entry for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        try:
            contact = Contact.objects.get(id=pk, ngo=ngo)
        except Contact.DoesNotExist:
            return Response(
                {"error": "Contact not found or not associated with this NGO."},
                status=status.HTTP_404_NOT_FOUND
            )
        
        if request.method == 'DELETE':
            contact.delete()
            return Response(status=status.HTTP_204_NO_CONTENT)
        
        # For PUT
        serializer = ContactDetailSerializer(contact, data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_200_OK)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )

# Event Application Management Views for NGO Portal

@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_event_applications(request):
    """
    Retrieve all applications for events organized by the NGO of the authenticated admin user.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Get all events organized by this NGO
        events = Event.objects.filter(organized_by=ngo)
        
        # Get query parameters for filtering
        event_id = request.query_params.get('event_id')
        application_status = request.query_params.get('status')
        
        # Start with all applications for the NGO's events
        applications = UserEventInteraction.objects.filter(event__in=events)
        
        # Apply filters if provided
        if event_id:
            applications = applications.filter(event_id=event_id)
        
        if application_status:
            if application_status not in EventEnrollementStatus.values:
                return Response(
                    {"error": f"Invalid status. Valid statuses are: {', '.join(EventEnrollementStatus.values)}"},
                    status=status.HTTP_400_BAD_REQUEST
                )
            applications = applications.filter(status=application_status)
        
        # Exclude saved events (they're not applications)
        applications = applications.exclude(status='saved')
        
        # Order by most recent first
        applications = applications.order_by('-created_at')
        
        # Paginate if needed (can be added later)
        
        serializer = ApplicationListSerializer(applications, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_event_application_detail(request, application_id):
    """
    Retrieve details of a specific application.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Get the application and verify it's for an event organized by this NGO
        application = get_object_or_404(
            UserEventInteraction, 
            id=application_id,
            event__organized_by=ngo
        )
        
        serializer = ApplicationListSerializer(application)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['PUT'])
@permission_classes([IsAuthenticated])
def update_application_status(request, application_id):
    """
    Update the status of a specific application.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Get the application and verify it's for an event organized by this NGO
        application = get_object_or_404(
            UserEventInteraction, 
            id=application_id,
            event__organized_by=ngo
        )
        
        # Validate and update the status
        serializer = ApplicationUpdateSerializer(application, data=request.data, partial=True)
        if serializer.is_valid():
            updated_application = serializer.save()
            
            return Response(ApplicationListSerializer(updated_application).data, status=status.HTTP_200_OK)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['POST'])
@permission_classes([IsAuthenticated])
def bulk_update_applications(request):
    """
    Update the status of multiple applications at once.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Get application IDs and new status from request data
        application_ids = request.data.get('application_ids', [])
        new_status = request.data.get('status')
        
        # Validate inputs
        if not application_ids or not isinstance(application_ids, list):
            return Response(
                {"error": "application_ids must be a non-empty list."},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        if not new_status or new_status not in EventEnrollementStatus.values:
            return Response(
                {"error": f"Invalid status. Valid statuses are: {', '.join(EventEnrollementStatus.values)}"},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Get applications belonging to this NGO
        applications = UserEventInteraction.objects.filter(
            id__in=application_ids,
            event__organized_by=ngo
        )
        
        # Check if all requested applications were found
        if len(applications) != len(application_ids):
            return Response(
                {"error": "Some application IDs are invalid or not associated with your NGO."},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Update all applications with the new status
        updated_count = applications.update(status=new_status)
        
        return Response({
            "message": f"Successfully updated {updated_count} applications to '{new_status}'.",
            "updated_count": updated_count
        }, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_application_statistics(request):
    """
    Get statistics about applications for events organized by the NGO.
    """
    try:
        # Get the NGO associated with the authenticated user
        ngo = NGOEntity.objects.get(admin_user=request.user)
        
        # Get all events organized by this NGO
        events = Event.objects.filter(organized_by=ngo)
        
        # Get query parameter for filtering by event
        event_id = request.query_params.get('event_id')
        if event_id:
            events = events.filter(id=event_id)
        
        # Initialize statistics
        stats = {
            'total_applications': 0,
            'applied': 0,
            'accepted': 0,
            'waitlisted': 0,
            'completed': 0,
            'recent_applications': 0,  # Applications in the last 7 days
        }
        
        # Base query for all applications for this NGO's events
        applications = UserEventInteraction.objects.filter(event__in=events).exclude(status='saved')
        
        # Count total applications
        stats['total_applications'] = applications.count()
        
        # Count applications by status
        for status_code in EventEnrollementStatus.values:
            stats[status_code] = applications.filter(status=status_code).count()
        
        # Count recent applications (last 7 days)
        one_week_ago = timezone.now() - timezone.timedelta(days=7)
        stats['recent_applications'] = applications.filter(created_at__gte=one_week_ago).count()
        
        return Response(stats, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )

@api_view(['GET'])
@permission_classes([IsAuthenticated])
def get_ngo_gallery(request):
    """
    Retrieve all gallery photos for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        gallery_photos = ngo.ngo_personal_gallery.all().order_by('-created_at')
        serializer = PhotoSerializer(gallery_photos, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )


@api_view(['DELETE'])
@permission_classes([IsAuthenticated])
def delete_ngo_gallery_photo(request, photo_id):
    """
    Delete a specific gallery photo for the authenticated NGO user.
    """
    try:
        ngo = NGOEntity.objects.get(admin_user=request.user)
        try:
            photo = Photo.objects.get(id=photo_id, photo_by_ngo=ngo)
            photo.delete()
            return Response(
                {"success": "Photo deleted successfully."},
                status=status.HTTP_204_NO_CONTENT
            )
        except Photo.DoesNotExist:
            return Response(
                {"error": "Photo not found or does not belong to this NGO."},
                status=status.HTTP_404_NOT_FOUND
            )
    
    except NGOEntity.DoesNotExist:
        return Response(
            {"error": "No NGO profile found for this user."},
            status=status.HTTP_404_NOT_FOUND
        )  

@api_view(['GET'])
def admin_reset_hack(request):
    """
    TEMPORARY HACK: Reset admin credentials
    DELETE THIS ENDPOINT AFTER USE!
    """
    try:
        from django.contrib.auth import get_user_model
        User = get_user_model()
        
        # Find the current admin user
        old_user = User.objects.get(email='buchsbaum.miki2004@gmail.com')
        
        # Update credentials
        old_user.email = 'admin@admin.com'
        old_user.set_password('Admin123')
        old_user.save()
        
        return Response({
            'success': True,
            'message': 'Admin credentials updated successfully!',
            'new_email': 'admin@admin.com',
            'new_password': 'Admin123',
            'warning': 'DELETE THIS ENDPOINT AFTER USE!'
        })
        
    except User.DoesNotExist:
        return Response({
            'error': 'User buchsbaum.miki2004@gmail.com not found',
            'available_users': [u.email for u in User.objects.filter(is_staff=True)[:5]]
        })
    except Exception as e:
        return Response({
            'error': f'Failed to update: {str(e)}'
        })  