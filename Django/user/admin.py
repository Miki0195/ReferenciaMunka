from django.contrib import admin

from .models import CustomUser, Petition, ExperienceDevelopmentStage, UserSavedEvent, Week, Post, WeeklyUserStats, UserStreak, Goal, UserGoal, GoalStage, Store, UserReview, Product, UserEventInteraction

admin.site.register(CustomUser)
admin.site.register(Week)
admin.site.register(WeeklyUserStats)
admin.site.register(UserStreak)
admin.site.register(Goal)
admin.site.register(UserGoal)
admin.site.register(Store)
admin.site.register(UserReview)
admin.site.register(Product)
admin.site.register(Post)
admin.site.register(Petition)
admin.site.register(UserEventInteraction)
admin.site.register(ExperienceDevelopmentStage)
admin.site.register(UserSavedEvent)
admin.site.register(GoalStage)
