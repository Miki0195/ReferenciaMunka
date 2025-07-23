from rest_framework.decorators import api_view
from rest_framework.response import Response

from shared.models import Team, Competences
from shared.serializers import TeamSerializer
from user.models import CustomUser
from .models import Event, Location
from .serializers import CompetencesSerializer, EventSerializer, MapMarkerSerializer, LocationSerializer, PhotoSerializer
from datetime import datetime
from rest_framework import status
from django.utils.timezone import make_aware
from django.utils import timezone
from shared.utils import format_datetime

@api_view(['GET'])
def getRoutes(request):
    routes = [
    {
        'Endpoint': '/events/',
        'method': 'GET',
        'body': None,
        'description': 'Returns an array of events'
    },
      {
        'Endpoint': '/events/id',
        'method': 'GET',
        'body': None,
        'description': 'Returns a single event object'
    },
      {
        'Endpoint': '/events/create/',
        'method': 'POST',
        'body': {},
        'description': 'Creates a new event with data sent in POST request'
    },
    {
        'Endpoint': '/events/id/update/',
        'method': 'PUT',
        'body': {},
        'description': 'Creates an existing event with data sent in POST request'
    },
    {
        'Endpoint': '/events/id/delete',
        'method': 'DELETE',
        'body': None,
        'description': 'Deletes an existing event'
    }
    ]
    return Response(routes)

@api_view(['GET'])
def getMapMarkers(request):
    events = Event.objects.all()
    serializer = MapMarkerSerializer(events, many = True)
    return Response(serializer.data)

@api_view(['GET'])
def getEvent(request, pk):
    event = Event.objects.prefetch_related('event_gallery').get(id=pk)

    serializer = EventSerializer(event, many = False)
    return Response(serializer.data)

@api_view(['POST'])
def createEvent(request):
    """
    Create a new event
    """
    serializer = EventSerializer(data=request.data)
    if serializer.is_valid():
        event = serializer.save()
        return Response(EventSerializer(event, many=False).data, status=status.HTTP_201_CREATED)
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

@api_view(['PUT'])
def updateEvent(request, pk):
    """
    Update an existing event
    """
    try:
        event = Event.objects.get(id=pk)
        serializer = EventSerializer(event, data=request.data, partial=True)
        if serializer.is_valid():
            updated_event = serializer.save()
            return Response(EventSerializer(updated_event).data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    except Event.DoesNotExist:
        return Response({"error": "Event not found"}, status=status.HTTP_404_NOT_FOUND)
    except Exception as e:
        print(f"Unexpected error in updateEvent: {e}")
        import traceback
        traceback.print_exc()
        return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['DELETE'])
def deleteEvent(request, pk):
    event = Event.objects.get(id=pk)
    event.delete()
    return Response("The event was deleted")

@api_view(['GET'])
def filter_upcoming_events(request, user_id):
    """
    Filter events with start_date later than the provided current_date.
    """
    current_date = request.GET.get('current_date', timezone.now().strftime("%Y-%m-%d"))  # YYYY-MM-DD

    user = CustomUser.objects.get(id=user_id)

    try:
        current_date = make_aware(datetime.strptime(current_date, "%Y-%m-%d"))
        print("Current Date:", current_date)
    except ValueError:
        return Response(
            {"Error": "current_date must be in the format YYYY-MM-DD."},
            status=status.HTTP_400_BAD_REQUEST
        )

    events = Event.objects.filter(startDate__gt=current_date)
    serializer = EventSerializer(events, many=True, context={"user": user})

    return Response(serializer.data, status=status.HTTP_200_OK)


@api_view(['GET'])
def get_events_filtering_options(request):
    locations = Location.objects.all()
    teams = Team.objects.all()
    competences = Competences.objects.all()

    location_serializer = LocationSerializer(locations, many=True)
    team_serializer = TeamSerializer(teams, many=True)
    competences_serializer = CompetencesSerializer(competences, many=True)

    return Response({
        'locations': location_serializer.data,
        'teams': team_serializer.data, 
        'competences': competences_serializer.data
    })

@api_view(['GET'])
def get_event_gallery(request, event_id):
    try:
        event = Event.objects.prefetch_related('event_gallery').get(id=event_id)

        photos = event.event_gallery.all()

        serializer = PhotoSerializer(photos, many=True)

        return Response(serializer.data, status=status.HTTP_200_OK)
    except Event.DoesNotExist:
        return Response({"error": "Event not found."}, status=status.HTTP_404_NOT_FOUND)

@api_view(['GET'])
def get_events_by_ngo(request, ngo_id):
    try:
        events = Event.objects.filter(organized_by_id=ngo_id)
        serializer = EventSerializer(events, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
    except Exception as e:
        return Response({"error": str(e)}, status=status.HTTP_400_BAD_REQUEST)
        