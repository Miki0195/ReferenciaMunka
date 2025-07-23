from django.apps import AppConfig

class NgoConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'ngo'

    def ready(self):
        """
        Import and register signals when the app is ready.
        """
        import ngo.signals  # Import the signals module to register signals

        import user.signals  