from django.contrib import admin
from django.contrib.admin import AdminSite
from backend import settings
from events.models import Event
from ngo.models import NGOEntity
from user.models import UserEventInteraction
from shared.models import Location
from django.core.mail import send_mail
from django.template.loader import render_to_string

class NGOAdminSite(AdminSite):
    site_header = "NGO Admin Panel"
    site_title = "NGO Admin"
    index_title = "Welcome to the NGO Admin Panel"

ngo_admin_site = NGOAdminSite(name='ngo_admin')

class UserEventInteractionAdmin(admin.ModelAdmin):
    list_display = ('user', 'event', 'status', 'created_at')
    list_filter = ('status', 'event')
    search_fields = ('user__username', 'event__eventName')
    actions = ['mark_accepted', 'mark_waitlisted']

    def mark_accepted(self, request, queryset):
        updated_count = queryset.update(status='accepted')
        for application in queryset:
            subject = f'Your Application Has Been Accepted: {application.event.eventName}'
            message = render_to_string('user/emails/application_accepted_email.txt', {
                'event': application.event,
                'user': application.user,
            })
            send_mail(
                subject,
                message,
                settings.EMAIL_HOST_USER,
                [application.user.email],
                fail_silently=False,
            )
        self.message_user(request, f"{updated_count} applications were marked as accepted.")
    mark_accepted.short_description = "Mark selected applications as Accepted"

    def mark_waitlisted(self, request, queryset):
        queryset.update(status='waitlisted')
    mark_waitlisted.short_description = "Mark selected applications as Waitlisted"

    def get_queryset(self, request):
        # Restrict the queryset to applications for events organized by the logged-in NGO
        qs = super().get_queryset(request)
        if request.user.is_superuser:
            return qs.exclude(status='saved') 
        # Filter events by the NGO associated with the logged-in user
        return qs.filter(event__organized_by=request.user.ngoentity).exclude(status='saved')

    def has_change_permission(self, request, obj=None):
        return request.user.has_perm('yourapp.can_change_status')

ngo_admin_site.register(UserEventInteraction, UserEventInteractionAdmin)
ngo_admin_site.register(Event)
ngo_admin_site.register(NGOEntity)
ngo_admin_site.register(Location)