from django.contrib import admin

# Register your models here.
from .models import NGOEntity, SocialMedia, Contact

admin.site.register(NGOEntity)
admin.site.register(SocialMedia)
admin.site.register(Contact)