from django.contrib import admin

# Register your models here.

from .models import Achievements, Event, TRRDonation

admin.site.register(Event)
admin.site.register(Achievements)
admin.site.register(TRRDonation)