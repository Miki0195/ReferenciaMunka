"""
Script to associate a user with an NGO entity.
Usage: python associate_ngo_user.py <user_email> <ngo_id>
"""
import os
import sys
import django

# Set up Django environment
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'backend.settings')
django.setup()

from user.models import CustomUser
from ngo.models import NGOEntity

def associate_user_with_ngo(user_email, ngo_id):
    try:
        user = CustomUser.objects.get(email=user_email)
    except CustomUser.DoesNotExist:
        print(f"Error: User with email {user_email} does not exist.")
        return False
    
    try:
        ngo = NGOEntity.objects.get(id=ngo_id)
    except NGOEntity.DoesNotExist:
        print(f"Error: NGO with ID {ngo_id} does not exist.")
        return False
    
    ngo.admin_user = user
    ngo.save()
    print(f"Successfully associated user {user.email} with NGO {ngo.name} (ID: {ngo.id}).")
    return True

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python associate_ngo_user.py <user_email> <ngo_id>")
        sys.exit(1)
    
    user_email = sys.argv[1]
    ngo_id = sys.argv[2]
    
    success = associate_user_with_ngo(user_email, ngo_id)
    sys.exit(0 if success else 1) 