"""
URL configuration for Naturalize project.
This is the general routing
"""
from django.contrib import admin
from django.urls import path, include

urlpatterns = [
    path('admin/', admin.site.urls),
    path('', include('events.urls')),
    path('', include('user.urls')),
    path('', include('ngo.urls')),
    path('', include('shared.urls')),
]
