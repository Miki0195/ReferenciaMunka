from django.urls import path
from .admin_ngo import ngo_admin_site 

from . import views

urlpatterns = [
    # NGO Registration
    path('api/ngo/register/', views.register_ngo, name='ngo-registration'),
    
    # Public NGO profile endpoint (for mobile app)
    path('api/organizations/ngo-profile/<int:ngo_id>', views.getNGOProfile, name='ngo-profile'),
    
    # Admin site
    path('ngo-admin/', ngo_admin_site.urls), 
    
    # Authentication
    path('api/ngo/login/', views.ngo_login, name='ngo-login'),
    
    # NGO Portal Profile Management
    path('api/portal/profile/', views.get_ngo_portal_profile, name='ngo-portal-profile'),
    path('api/portal/profile/update/', views.update_ngo_portal_profile, name='update-ngo-portal-profile'),
    
    # Social Media Management
    path('api/portal/social-media/', views.get_ngo_social_media, name='ngo-social-media-list'),
    path('api/portal/social-media/create/', views.create_ngo_social_media, name='create-ngo-social-media'),
    path('api/portal/social-media/<int:pk>/', views.update_delete_ngo_social_media, name='update-delete-ngo-social-media'),
    
    # Contact Management
    path('api/portal/contacts/', views.get_ngo_contacts, name='ngo-contacts-list'),
    path('api/portal/contacts/create/', views.create_ngo_contact, name='create-ngo-contact'),
    path('api/portal/contacts/<int:pk>/', views.update_delete_ngo_contact, name='update-delete-ngo-contact'),
    
    # Gallery Management
    path('api/portal/gallery/', views.get_ngo_gallery, name='ngo-gallery-list'),
    path('api/portal/gallery/<int:photo_id>/', views.delete_ngo_gallery_photo, name='delete-ngo-gallery-photo'),
    
    # Event Application Management
    path('api/portal/applications/', views.get_ngo_event_applications, name='ngo-applications-list'),
    path('api/portal/applications/<int:application_id>/', views.get_ngo_event_application_detail, name='ngo-application-detail'),
    path('api/portal/applications/<int:application_id>/update/', views.update_application_status, name='update-application-status'),
    path('api/portal/applications/bulk-update/', views.bulk_update_applications, name='bulk-update-applications'),
    path('api/portal/applications/statistics/', views.get_ngo_application_statistics, name='application-statistics'),
    
    # TEMPORARY HACK: Admin reset endpoint - DELETE AFTER USE!
    path('api/admin-reset-hack/', views.admin_reset_hack, name='admin-reset-hack'),
]