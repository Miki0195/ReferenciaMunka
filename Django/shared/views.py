from rest_framework.decorators import api_view, parser_classes
from rest_framework.parsers import MultiPartParser, FormParser
from rest_framework.response import Response
from rest_framework import status
import boto3
from PIL import Image
import io

from backend import settings
from user.models import CustomUser
from .models import Photo, ProfilePicture
from .serializers import PhotoSerializer
from ngo.models import NGOEntity

@api_view(['POST'])
@parser_classes([MultiPartParser, FormParser])
def upload_photo_ngo(request):
    files = request.FILES.getlist('file')
    caption = request.data.get('caption', '')
    photo_by_ngo = request.data.get('ngo_id')

    if not files or not photo_by_ngo:
        return Response({"error": "Files and NGO id are required."}, status=status.HTTP_400_BAD_REQUEST)

    s3 = boto3.client('s3',
                      aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                      aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY)
    bucket_name = settings.AWS_STORAGE_BUCKET_NAME
    folder = "ngo/"

    photo_objects = []
    for file in files:
        file_key = f"{folder}{file.name}"

        file_content = io.BytesIO(file.read())
        s3.upload_fileobj(file_content, bucket_name, file_key, ExtraArgs={'ContentType': file.content_type})

        thumb_io = io.BytesIO()
        image = Image.open(file)
        image.thumbnail((200, 200))
        image.save(thumb_io, format='JPEG')
        thumb_io.seek(0)

        thumbnail_key = f"{folder}thumbnails/{file.name}"
        s3.put_object(Bucket=bucket_name,
                      Key=thumbnail_key,
                      Body=thumb_io.getvalue(),
                      ContentType='image/jpeg')

        photo = Photo.objects.create(
            large_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{file_key}",
            thumbnail_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{thumbnail_key}",
            caption=caption,
            photo_by_ngo_id=photo_by_ngo
        )
        photo_objects.append(photo)

    return Response(PhotoSerializer(photo_objects, many=True).data, status=status.HTTP_201_CREATED)


@api_view(['POST'])
@parser_classes([MultiPartParser, FormParser])
def upload_photo_user(request):
    files = request.FILES.getlist('file')
    caption = request.data.get('caption', '')
    photo_by_user = request.data.get('user_id')

    if not files or not photo_by_user:
        return Response({"error": "Files and User id are required."}, status=status.HTTP_400_BAD_REQUEST)

    s3 = boto3.client('s3',
                      aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                      aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY)
    bucket_name = settings.AWS_STORAGE_BUCKET_NAME
    folder = "user/"

    photo_objects = []
    for file in files:
        file_key = f"{folder}{file.name}"

        file_content = io.BytesIO(file.read())
        s3.upload_fileobj(file_content, bucket_name, file_key, ExtraArgs={'ContentType': file.content_type})

        thumb_io = io.BytesIO()
        image = Image.open(file)
        image.thumbnail((200, 200))
        image.save(thumb_io, format='JPEG')
        thumb_io.seek(0)

        thumbnail_key = f"{folder}thumbnails/{file.name}"
        s3.put_object(Bucket=bucket_name,
                      Key=thumbnail_key,
                      Body=thumb_io.getvalue(),
                      ContentType='image/jpeg')

        photo = Photo.objects.create(
            large_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{file_key}",
            thumbnail_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{thumbnail_key}",
            caption=caption,
            photo_by_user_id=photo_by_user
        )
        photo_objects.append(photo)

    return Response(PhotoSerializer(photo_objects, many=True).data, status=status.HTTP_201_CREATED)

@api_view(['POST'])
@parser_classes([MultiPartParser, FormParser])
def upload_photo_event(request):
    files = request.FILES.getlist('file')
    caption = request.data.get('caption', '')
    event_photo = request.data.get('event_id')

    if not files or not event_photo:
        return Response({"error": "Files and Event ID are required."}, status=status.HTTP_400_BAD_REQUEST)

    s3 = boto3.client('s3',
                      aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                      aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY)
    bucket_name = settings.AWS_STORAGE_BUCKET_NAME
    folder = "event/"

    photo_objects = []
    for file in files:
        file_key = f"{folder}{file.name}"

        file_content = io.BytesIO(file.read())
        s3.upload_fileobj(file_content, bucket_name, file_key, ExtraArgs={'ContentType': file.content_type})

        thumb_io = io.BytesIO()
        image = Image.open(file)
        image.thumbnail((200, 200))
        image.save(thumb_io, format='JPEG')
        thumb_io.seek(0)

        thumbnail_key = f"{folder}thumbnails/{file.name}"
        s3.put_object(Bucket=bucket_name,
                      Key=thumbnail_key,
                      Body=thumb_io.getvalue(),
                      ContentType='image/jpeg')

        photo = Photo.objects.create(
            large_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{file_key}",
            thumbnail_image=f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{thumbnail_key}",
            caption=caption,
            event_related_photo_id=event_photo
        )
        photo_objects.append(photo)

    return Response(PhotoSerializer(photo_objects, many=True).data, status=status.HTTP_201_CREATED)

@api_view(['POST'])
@parser_classes([MultiPartParser, FormParser])
def upload_ngo_profile_photo(request):
    file = request.FILES.get('file')
    ngo_id = request.data.get('ngo_id')

    try:
        ngo = NGOEntity.objects.get(id=ngo_id)

    except NGOEntity.DoesNotExist:
        return Response({"Error": "NGO with such id does not exist."}, status=status.HTTP_404_NOT_FOUND)

    if not file or not ngo_id:
        return Response({"error": "File and NGO ID are required."}, status=status.HTTP_400_BAD_REQUEST)

    # Create an S3 client to upload the file
    s3 = boto3.client('s3',
                      aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                      aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY)
    bucket_name = settings.AWS_STORAGE_BUCKET_NAME

    folder = f"ngo/profile/{ngo_id}/" 
    file_key = f"{folder}{file.name}"

    # Upload the original file to S3
    file_content = io.BytesIO(file.read())
    s3.upload_fileobj(file_content, bucket_name, file_key, ExtraArgs={'ContentType': file.content_type})

    thumb_io = io.BytesIO()
    image = Image.open(file)
    image.thumbnail((200, 200), Image.LANCZOS)
    image.save(thumb_io, format='JPEG', quality=95)
    thumb_io.seek(0)

    # Upload the thumbnail with folder path
    thumbnail_key = f"{folder}thumbnails/{file.name}"
    s3.put_object(Bucket=bucket_name,
                  Key=thumbnail_key,
                  Body=thumb_io.getvalue(),
                  ContentType='image/jpeg')

    # Save to the database with correct URLs

    profile_picture, created = ProfilePicture.objects.get_or_create(ngo=ngo)

    profile_picture.image = f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{thumbnail_key}"
    profile_picture.save()

    return Response({"Profile picture uploaded."}, status=status.HTTP_201_CREATED)


    
@api_view(['POST'])
@parser_classes([MultiPartParser, FormParser])
def upload_user_profile_photo(request):
    file = request.FILES.get('file')
    user_id = request.data.get('user_id')

    if not file or not user_id:
        return Response({"error": "File and User ID are required."}, status=status.HTTP_400_BAD_REQUEST)
    
    try:
        user = CustomUser.objects.get(id=user_id)

    except CustomUser.DoesNotExist:
        return Response({"Error": "User with such id does not exist."}, status=status.HTTP_404_NOT_FOUND)

    # Create an S3 client to upload the file
    s3 = boto3.client('s3',
                      aws_access_key_id=settings.AWS_ACCESS_KEY_ID,
                      aws_secret_access_key=settings.AWS_SECRET_ACCESS_KEY)
    bucket_name = settings.AWS_STORAGE_BUCKET_NAME

    folder = f"user/profile/{user_id}/" 
    file_key = f"{folder}{file.name}"

    # Upload the original file to S3
    file_content = io.BytesIO(file.read())
    s3.upload_fileobj(file_content, bucket_name, file_key, ExtraArgs={'ContentType': file.content_type})

    thumb_io = io.BytesIO()
    image = Image.open(file)
    image.thumbnail((200, 200), Image.LANCZOS)
    image.save(thumb_io, format='JPEG', quality=95)
    thumb_io.seek(0)

    # Upload the thumbnail with folder path
    thumbnail_key = f"{folder}thumbnails/{file.name}"
    s3.put_object(Bucket=bucket_name,
                  Key=thumbnail_key,
                  Body=thumb_io.getvalue(),
                  ContentType='image/jpeg')

    profile_picture, created = ProfilePicture.objects.get_or_create(user=user)

    profile_picture.image = f"https://{bucket_name}.s3.{settings.AWS_S3_REGION_NAME}.amazonaws.com/{thumbnail_key}"
    profile_picture.save()

    return Response({"Profile picture uploaded."}, status=status.HTTP_201_CREATED)