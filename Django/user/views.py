from django.utils import timezone
from pstats import Stats, StatsProfile
from rest_framework.views import APIView
from rest_framework.decorators import api_view
from events.models import Achievements, Event
from events.serializers import AchievementsSerializer, EventSerializer, UserEventsCalendar
from shared.models import Competences
from user.models import CustomUser, Goal, Petition, Post, UserEventInteraction, UserGoal, WeeklyUserStats, UserStreak, Week, UserSavedEvent
from .serializers import CompetencesSerializer, CustomTokenObtainPairSerializer, CustomUserSerializer, GoalSerializer, PetitionSerializer, PostSerializer, UserEditSerializer, UserGoalSerializer, UserSerializer, UserStatsSerializer, UserStreakSerializer, UserXPSerializer
from rest_framework.response import Response
from rest_framework import status
from shared.constants import EventEnrollementStatus
from rest_framework.permissions import IsAuthenticated
from rest_framework.decorators import permission_classes
from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework.exceptions import NotFound

class CustomTokenObtainPairView(TokenObtainPairView):
    serializer_class = CustomTokenObtainPairSerializer

# view for registering users
class RegisterView(APIView):
    def post(self, request):
        serializer = UserSerializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        serializer.save()
        return Response(serializer.data)

class UserStatsView(APIView):
    def post(self, request):
        # Get the current date
        current_date = timezone.now()
        data = request.data
        user_id = data.get('user_id')
        
        # Find the current season based on the current date
        current_week = Week.objects.filter(start__lte=current_date, end__gte=current_date).first()

        if current_week:
            # Retrieve UserStats data for the user and the current season
            user_stats = WeeklyUserStats.objects.filter(user_id=user_id, week=current_week).first()
            user_streak = UserStreak.objects.filter(user_id=user_id).first()
            user = CustomUser.objects.filter(pk=user_id).first()

            #achievements_gained = CompletedEvents.objects.filter(user_id=user_id).select_related('event').prefetch_related('event__achievements')
            achievements_gained = Achievements.objects.filter(event_achv__completed_event__user=user_id).distinct().values('description', 'id', 'name', 'xpGiven')
            competences_gained = Competences.objects.filter(user_gained_competences__id=user_id)

            user_stats_serializer = UserStatsSerializer(user_stats)
            user_streak_serializer = UserStreakSerializer(user_streak)
            custom_user_serializer = UserXPSerializer(user)
            achievements_serializer = AchievementsSerializer(achievements_gained, many=True)
            competences_serializer = CompetencesSerializer(competences_gained, many=True)

            # Combine both serializer data into a single dictionary
            serialized_data = {
                  'current_league': current_week.league,
                  'current_week_xp': user_stats_serializer.data.get('xp_earned'),
                  'current_week_rank': user_stats_serializer.data.get('rank'),
                  'current_streak': user_streak_serializer.data.get('current_streak'),  # Extract current_streak from serializer data
                  'total_xp': custom_user_serializer.data.get('total_xp'),
                  'number_top_finishes': custom_user_serializer.data.get('number_top_finishes'),
                  'user_achievements': achievements_serializer.data,
                  'user_competences': competences_serializer.data
            }
            return Response(serialized_data)
        else:
            return Response({'error': 'WeeklyUserStats not found'}, status=StatsProfile.HTTP_404_NOT_FOUND)

# returns all posts from user's friends
def get_followed_users_posts(user_id):
    # Fetch the user object
    user = CustomUser.objects.get(pk=user_id)
    
    # Get posts created by users that the current user follows
    followed_users_posts = Post.objects.filter(created_by_user__in=user.following.all())

    return followed_users_posts

class UserFeed(APIView):
    def post(self, request):
        data = request.data
        user_id = data.get('user_id')

        followed_users_posts = get_followed_users_posts(user_id=user_id)
        serializer = PostSerializer(followed_users_posts, many=True)
        return Response(serializer.data)

class GetAllEvents(APIView):
    def post(self, request):
        data = request.data
        user_id = data.get('user_id')
        try:
            user = CustomUser.objects.get(id=user_id)
        except CustomUser.DoesNotExist:
            return Response({"error": "User not found"}, status=Stats.HTTP_404_NOT_FOUND)

        events = Event.objects.all()  # Assuming you want to check all events. Adjust the queryset if needed.

        # Use the serializer with the user context
        serializer = UserEventsCalendar(events, many=True, context={'user': user})

        return Response(serializer.data)

@api_view(['GET'])
def get_user_profile(request, user_id):
    try:
        user = CustomUser.objects.get(id=user_id)
    except CustomUser.DoesNotExist:
        return Response({"Error": "User not found"}, status=status.HTTP_404_NOT_FOUND)

    serializer = CustomUserSerializer(user)

    return Response(serializer.data, status=status.HTTP_200_OK)

class GetPetitions(APIView):
    def post(self, request):
        data = request.data
        print(data)
        user_id = data.get('user_id')
        try:
            user = CustomUser.objects.get(id=user_id)
        except CustomUser.DoesNotExist:
            return Response({"error": "User not found"}, status=Stats.HTTP_404_NOT_FOUND)

        petitions = Petition.objects.all()
        
        serializer = PetitionSerializer(petitions, many=True, context={'user': user})
        return Response(serializer.data)

@api_view(['GET'])
def get_user_active_goal(request, user_id):

    current_goal = UserGoal.objects.filter(user_id=user_id, status=1).first()

    if not current_goal:
            raise NotFound("There are no active goals for this user.")

    serializer = UserGoalSerializer(current_goal)

    return Response(serializer.data, status=status.HTTP_200_OK)

@api_view(['GET'])
def get_animal_collection(request):
    all_goals = Goal.objects.all()

    serializer = GoalSerializer(all_goals, many=True)

    return Response(serializer.data, status=status.HTTP_200_OK)

@api_view(['POST'])
def apply_to_event(request, event_id, user_id):
    user = CustomUser.objects.get(id=user_id) 
    try:
        event = Event.objects.get(id=event_id)
    except Event.DoesNotExist:
        return Response({"error": "Event not found"}, status=status.HTTP_404_NOT_FOUND)

    application_status = UserEventInteraction.objects.filter(
        user=user, event=event
    ).exclude(status='saved').first()
    
    if application_status:
        return Response(
                {"message": f"Your event status is: ({application_status.status})."},
                status=status.HTTP_200_OK,
            )

    UserEventInteraction.objects.create(user=user, event=event, status='applied')
    
    return Response({"Success": "You have successfully applied to the event."}, status=status.HTTP_201_CREATED)

@api_view(['POST'])
def save_event(request, event_id, user_id):
    try:
        user = CustomUser.objects.get(id=user_id)
    except CustomUser.DoesNotExist:
        return Response({"Error": "User not found"}, status=status.HTTP_404_NOT_FOUND)

    try:
        event = Event.objects.get(id=event_id)
    except Event.DoesNotExist:
        return Response({"error": "Event not found"}, status=status.HTTP_404_NOT_FOUND)

    saved_status = UserSavedEvent.objects.filter(user=user, event=event).first()

    if saved_status:
        return Response(
            {"message": "You have already saved this event."},
            status=status.HTTP_200_OK
        )

    UserSavedEvent.objects.create(user=user, event=event)

    return Response({"Message": "Event successfully saved."}, status=status.HTTP_201_CREATED)

@api_view(['GET'])
def unsave_event(request, event_id, user_id):
    try:
        user = CustomUser.objects.get(id=user_id)
    except CustomUser.DoesNotExist:
        return Response({"Error": "User not found"}, status=status.HTTP_404_NOT_FOUND)

    try:
        event = Event.objects.get(id=event_id)
    except Event.DoesNotExist:
        return Response({"error": "Event not found"}, status=status.HTTP_404_NOT_FOUND)

    saved_event = UserSavedEvent.objects.filter(user=user, event=event).first()

    if saved_event:
        saved_event.delete()
        return Response({"Message": "Event was successfully unsaved."}, status=status.HTTP_200_OK)
    else:
        return Response({"Message": "This event was not saved before."}, status=status.HTTP_404_NOT_FOUND)

@api_view(['GET'])
def delete_user(request, user_id):
    try:
        user = CustomUser.objects.get(id=user_id)
    except CustomUser.DoesNotExist:
        return Response({"Error": "User not found"}, status=status.HTTP_404_NOT_FOUND)

    if user:
        user.delete()
        return Response({"Message": "User profile was successfully deleted."}, status=status.HTTP_200_OK)


@api_view(['GET'])
def get_user_events(request, user_id):
    try:
        user = CustomUser.objects.get(id=user_id)
    except CustomUser.DoesNotExist:
        return Response({"Error": f"User with {user_id} does not exist"}, status=status.HTTP_404_NOT_FOUND)

    status_filter = request.query_params.get('status', None) 

    # Validate the status parameter
    if status_filter and status_filter not in EventEnrollementStatus.values:
        return Response(
            {"error": f"Invalid status. Valid statuses are: {', '.join(EventEnrollementStatus.values)}"},
            status=status.HTTP_400_BAD_REQUEST,
        )

    events = Event.objects.filter(usereventinteraction__user=user)

    if status_filter:
        events = events.filter(usereventinteraction__status=status_filter)

    serializer = EventSerializer(events, many=True, context={"user": user})

    return Response(serializer.data, status=status.HTTP_200_OK)

@api_view(['PATCH'])
@permission_classes([IsAuthenticated])
def edit_user_profile(request):
    user = request.user

    serializer = UserEditSerializer(user, data=request.data, partial=True)
    if serializer.is_valid():
        serializer.save()
        return Response(serializer.data, status=status.HTTP_200_OK)
    
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

@api_view(['GET'])
def choose_user_goal(request, user_id, goal_id):
    try:
        if not goal_id and not user_id:
            return Response({'Error': 'goal_id and user_id are required'}, status=status.HTTP_400_BAD_REQUEST)

        goal = Goal.objects.get(id=goal_id)
        user = CustomUser.objects.get(id=user_id)

        if UserGoal.objects.filter(user=user, goal=goal, status=1).exists():
            return Response({'error': 'This goal is already active.'}, status=status.HTTP_400_BAD_REQUEST)

        user_goal = UserGoal.objects.create(
            user=user,
            goal=goal,
            xp_accumulated=0,
            status=1,
        )

        return Response({
            'status': 'Success',
            'message': f'You have chosen the goal: {goal.name}',
        }, status=status.HTTP_201_CREATED)

    except Goal.DoesNotExist:
        return Response({'error': 'Goal not found'}, status=status.HTTP_404_NOT_FOUND)
    except Exception as e:
        return Response({'error': str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['POST'])
def add_user_xp_to_goal(request):
    try:
        goal_id = request.data.get('goal_id')
        xp_to_add = request.data.get('xp_to_add')
        user_id = request.data.get('user_id')

        if not goal_id or not xp_to_add:
            return Response({'Error': 'goal_id and xp_to_add are required'}, status=status.HTTP_400_BAD_REQUEST)

        goal = Goal.objects.get(id=goal_id)
        user = CustomUser.objects.get(id=user_id)

        try:
            user_goal = UserGoal.objects.get(
                user=user,
                goal=goal,
                status=1
            )
        except UserGoal.DoesNotExist:
            return Response({'Error': f'No active goal with name {goal.name} exists for user with id {user.id}.'}, status=status.HTTP_404_NOT_FOUND)

        user_goal.xp_accumulated += xp_to_add

        if user_goal.xp_accumulated >= goal.xp_required:
            user_goal.status = 2
            user_goal.save()
            return Response({
                'status': 'completed',
                'goal_id': goal.id,
                'xp_accumulated': user_goal.xp_accumulated
            }, status=status.HTTP_200_OK)
        else:
            user_goal.save()
            return Response({
                'status': 'active',
                'goal_id': goal.id,
                'xp_accumulated': user_goal.xp_accumulated
            }, status=status.HTTP_200_OK)

    except Goal.DoesNotExist:
        return Response({'Error': 'Goal not found'}, status=status.HTTP_404_NOT_FOUND)
    except Exception as e:
        return Response({'Error': str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

@api_view(['POST'])
def update_streak_for_user(request):
    try:
        user_id = request.data.get('user_id')
        user = CustomUser.objects.get(id=user_id)
    
        streak, _ = UserStreak.objects.get_or_create(user=user)
        streak.update_streak()

        return Response({"Message": "Streak updated successfully."}, status=200)

    except CustomUser.DoesNotExist:
        return Response({"error": "User not found"}, status=404)
    except Exception as e:
        return Response({"error": str(e)}, status=500)