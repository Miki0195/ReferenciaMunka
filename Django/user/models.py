from django.db import models
from django.contrib.auth.models import AbstractUser, BaseUserManager
from events.models import Achievements, Event
from shared.constants import AnimalCategory, EventEnrollementStatus, BabyAnimals, DevelopmentStage
from .utils import add_xp_to_user
from django.core.validators import MinValueValidator, MaxValueValidator
from django.utils import timezone

class ExperienceDevelopmentStage(models.Model):
    name = models.CharField(max_length=50, choices=DevelopmentStage.choices)
    min_xp = models.PositiveIntegerField(default=0)
    max_xp = models.PositiveIntegerField(default=0)
    stage_level = models.PositiveIntegerField(
        default=1,
        validators=[MinValueValidator(1), MaxValueValidator(10)]
    )
  
    def __str__(self):
        return self.name

class UserManager(BaseUserManager):

    use_in_migration = True

    def create_user(self, email, password=None, **extra_fields):
        if not email:
            raise ValueError('Email is required')
        user = self.model(email=self.normalize_email(email), **extra_fields)
        user.set_password(password)
        user.save(using=self._db)
        return user

    def create_superuser(self, email, password, **extra_fields):
        extra_fields.setdefault('is_staff', True)
        extra_fields.setdefault('is_superuser', True)
        extra_fields.setdefault('is_active', True)

        if extra_fields.get('is_staff') is not True:
            raise ValueError('Superuser must have is_staff = True')
        if extra_fields.get('is_superuser') is not True:
            raise ValueError('Superuser must have is_superuser = True')

        return self.create_user(email, password, **extra_fields)


class CustomUser(AbstractUser):
    username = None  # Identify user by email instead of username
    email = models.EmailField(unique=True)
    name = models.CharField(max_length=100, blank=True, null=True)
    surname = models.CharField(max_length=100, null=True, blank=True)
    total_xp = models.PositiveIntegerField(default=0)
    date_joined = models.DateTimeField(auto_now_add=True)
    birthday = models.DateField(null=True, blank=True)
    bio = models.TextField(null=True, blank=True)
    number_top_finishes = models.PositiveIntegerField(default=0, blank=True)
    following = models.ManyToManyField('self', symmetrical=False, related_name='followers', null=True, blank=True, default=0)
    focus_teams = models.ManyToManyField('shared.Team', related_name='user_focus_teams', null=True, blank=True)
    gained_competences = models.ManyToManyField('shared.Competences', related_name='user_gained_competences', null=True, blank=True)
    gained_achievements = models.ManyToManyField(Achievements, related_name='user_gained_achievements', null=True, blank=True)
    created_events = models.ForeignKey(Event, on_delete=models.SET_NULL, null=True, blank=True, related_name='events_created_by_user')
    causes = models.CharField(max_length=1000, null=True, blank=True)
    expectations = models.CharField(max_length=1000, null=True, blank=True)
    current_stage = models.ForeignKey(
        ExperienceDevelopmentStage,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='users_in_stage' # name for the reverse relation
    )

    is_admin = models.BooleanField(default=False)
    is_active = models.BooleanField(default=True)
    is_staff = models.BooleanField(default=False)
    is_superuser = models.BooleanField(default=False)

    objects = UserManager()

    USERNAME_FIELD = 'email'
    REQUIRED_FIELDS = []

    def __str__(self):
        return self.name if self.name else self.email

    def save(self, *args, **kwargs):
        if self.current_stage:
            if not (self.current_stage.min_xp <= self.total_xp <= self.current_stage.max_xp):
                next_stage = ExperienceDevelopmentStage.objects.filter(
                    min_xp__lte=self.total_xp,
                    max_xp__gte=self.total_xp
                ).order_by('stage_level').first()

                if next_stage:
                    self.current_stage = next_stage

            if "update_fields" in kwargs and kwargs["update_fields"]:
                kwargs["update_fields"] = list(set(kwargs["update_fields"] + ["current_stage"]))

        super().save(*args, **kwargs)

class Petition(models.Model):
    title = models.TextField()
    created_by = models.ForeignKey(CustomUser, on_delete=models.CASCADE, related_name='petition_creator')
    saved_petitions_by_users = models.ManyToManyField(CustomUser, related_name='saved_user_petitions', null=True)
    avg_rating = models.FloatField()
    xp_given = models.IntegerField()
    start_date = models.DateField()
    about = models.TextField()
    # to be changed
    # impact_gallery = models.ForeignKey(PhotoBase, on_delete=models.CASCADE, related_name="petition_impact_gallery") 
    image_url = models.TextField()
    about_creator = models.TextField()
    total_signs = models.IntegerField(default=0)
    friends_that_signed = models.IntegerField(default=0)

class Week(models.Model):
    LEAGUE_CHOICES = [
        ('Bronze', 'Bronze'),
        ('Silver', 'Silver'),
        ('Gold', 'Gold'),
        ('Shell', 'Shell')
    ]

    start = models.DateField()
    end = models.DateField()
    league = models.CharField(max_length=20, choices=LEAGUE_CHOICES)

    def __str__(self):
        return f"{self.league} Week ({self.start} to {self.end})"

class WeeklyUserStats(models.Model):
    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE)
    week = models.ForeignKey(Week, on_delete=models.CASCADE)
    xp_earned = models.PositiveIntegerField(default=0)
    rank = models.PositiveIntegerField(null=True, blank=True)
    last_updated = models.DateTimeField(auto_now=True)

    def save(self, *args, **kwargs):
        # Save the current WeeklyUserStats instance
        super().save(*args, **kwargs)
        # Update the user's total XP
        self.user.total_xp = WeeklyUserStats.objects.filter(user=self.user).aggregate(models.Sum('xp_earned'))['xp_earned__sum'] or 0
        self.user.save()

    def __str__(self):
        return f"{self.user.username} - {self.week} - XP: {self.xp_earned}, Rank: {self.rank}"

class UserStreak(models.Model):
    user = models.OneToOneField(CustomUser, on_delete=models.CASCADE)
    current_streak = models.PositiveIntegerField(default=1)
    longest_streak = models.PositiveIntegerField(default=1)
    last_active_date = models.DateField(null=True, blank=True)

    def update_streak(self):
        today = timezone.now().date()

        print(f"Before update - Streak: {self.current_streak}, Last Active: {self.last_active_date}")  # Debugging

        if self.last_active_date == today - timezone.timedelta(days=1):
            self.current_streak += 1

        if self.current_streak > self.longest_streak:
            self.longest_streak = self.current_streak
        
        self.last_active_date = today

        self.save(update_fields=['current_streak', 'longest_streak', 'last_active_date'])

    def __str__(self):
        return f"{self.user.username} - Current Streak: {self.current_streak}, Longest Streak: {self.longest_streak}"

class Goal(models.Model):
    name = models.TextField(choices=BabyAnimals.choices)
    description = models.TextField()
    xp_required = models.IntegerField(default=0)
    image_url = models.TextField(blank=True, null=True)
    category = models.TextField(choices=AnimalCategory.choices, blank=True, null=True)

class GoalStage(models.Model):
    goal = models.ForeignKey(Goal, on_delete=models.CASCADE, related_name="stages")
    min_xp = models.PositiveIntegerField(default=0)
    max_xp = models.PositiveIntegerField(default=0)
    stage_level = models.PositiveIntegerField(
        default=1,
        validators=[MinValueValidator(1), MaxValueValidator(10)]
    )

    class Meta:
        unique_together = ('goal', 'min_xp', 'max_xp')

class UserGoal(models.Model):
    STATUS_CHOICES = [
        (1, "Active"),
        (2, "Completed"),
    ]

    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE)
    goal = models.ForeignKey(Goal, on_delete=models.CASCADE)
    xp_accumulated = models.PositiveIntegerField(default=0)
    status = models.IntegerField(choices=STATUS_CHOICES, default=1)
    level = models.IntegerField(default=1)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    current_stage = models.ForeignKey(
        GoalStage, on_delete=models.SET_NULL, null=True, blank=True
    )

    def is_completed(self):
        return self.xp_accumulated >= self.goal.xp_required

    def update_stage(self):
        stage = (
            self.goal.stages.filter(min_xp__lte=self.xp_accumulated, max_xp__gte=self.xp_accumulated)
            .order_by("-min_xp")
            .first()
        )

        print(f'This is the changed {self.xp_accumulated}')
        if stage and stage != self.current_stage:
            self.current_stage = stage

    def save(self, *args, **kwargs):
        self.status = 2 if self.xp_accumulated >= self.goal.xp_required else 1

        previous_stage = self.current_stage
        self.update_stage()

        if "update_fields" in kwargs and kwargs["update_fields"]:
            kwargs["update_fields"] = list(set(kwargs["update_fields"] + ["status", "current_stage", "xp_accumulated"]))

        super().save(*args, **kwargs)

class Store(models.Model):
    about = models.TextField()
    followers = models.ManyToManyField(CustomUser, related_name='store_followers')
    rating = models.IntegerField(default=0)
    store_goal = models.TextField()
    product_desc = models.TextField()
    store_location = models.OneToOneField('shared.Location', on_delete=models.CASCADE)

class Post(models.Model):
    description = models.TextField(null=True, blank=True)
    post_image = models.URLField(null=True, blank=True)
    created_by_user = models.ForeignKey(CustomUser, on_delete=models.CASCADE, null=True, blank=True)
    created_by_ngo = models.ForeignKey('ngo.NGOEntity', on_delete=models.CASCADE, null=True, blank=True)

class UserReview(models.Model):
    user_reviewer = models.ForeignKey(CustomUser, on_delete=models.CASCADE, null=True, blank=True)
    reviewed_ngo = models.ForeignKey('ngo.NGOEntity', on_delete=models.CASCADE, related_name="user_reviews", null=True, blank=True)
    reviewed_store = models.ForeignKey(Store, on_delete=models.CASCADE, null=True, blank=True)
    review_comment = models.TextField()
    review_rating = models.FloatField()
    date_reviewed = models.DateField(auto_now_add=True)

    class Meta:
        constraints = [
            models.CheckConstraint(
                check=(
                    models.Q(reviewed_ngo__isnull=False, reviewed_store__isnull=True) |
                    models.Q(reviewed_ngo__isnull=True, reviewed_store__isnull=False)
                ),
                name="only_one_reviewed_entity",
            )
        ]

class Product(models.Model):
    name = models.TextField()
    product_image = models.URLField(blank=True, null=True)
    description = models.TextField()

class UserSavedEvent(models.Model):
    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE)
    event = models.ForeignKey(Event, on_delete=models.CASCADE)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        unique_together = ('user', 'event')

class UserEventInteraction(models.Model):
    STATUS_POINTS = {
        'waitlisted': 1,
        'applied': 1,
        'accepted': 5,
        'completed': 50
    }

    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE)
    event = models.ForeignKey(Event, on_delete=models.CASCADE)
    status = models.CharField(max_length=10, choices=EventEnrollementStatus.choices)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        constraints = [
            models.UniqueConstraint(
                fields=['user', 'event'],
                name='unique_application_status_per_user_event',
            ),
        ]

    def save(self, *args, **kwargs):

        is_new = self.pk is None

        if not is_new:
            previous = UserEventInteraction.objects.filter(pk=self.pk).only('status').first()
            self._previous_status = previous.status if previous else None

        if is_new or (self.pk and hasattr(self, '_previous_status') and self._previous_status != self.status
        and self.status != EventEnrollementStatus.COMPLETED):
            xp_earned = self.STATUS_POINTS.get(self.status, 0)
            print(f"This is xp_earned {xp_earned}")
            if xp_earned > 0:
                add_xp_to_user(self.user, xp_earned)

        super().save(*args, **kwargs)