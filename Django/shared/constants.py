from django.db import models

STAGE_LEVELS = {
    1: "First stage",
    2: "Second stage",
    3: "Third stage",
    4: "Fourth stage",
}

DEVELOPMENT_STAGE = {
        1: "Eco Pioneer",
        2: "Biosphere Guardian",
        3: "Gaia's Emissary",
        4: "Planet Healer",
        5: "Cosmic Steward",
}

class MarkerType(models.IntegerChoices):
        VO = 1, "Volunteering opportunity",
        SE = 2, "Sustainable event",
        Donation = 3, "Donation"

class TeamChoices(models.IntegerChoices):
        INTO_THE_HEIGHTS = 1, 'INTO THE HEIGHTS'
        MARINES = 2, 'MARINES'
        GREEN_CITIES = 3, 'GREEN CITIES'
        ANCIENT_FORESTS_FOREVAA = 4, 'ANCIENT FORESTS FOREVAA'

class EventEnrollementStatus(models.TextChoices):
        APPLIED = 'applied', 'Applied'
        ACCEPTED = 'accepted', 'Accepted'
        WAITLISTED = 'waitlisted', 'Waitlisted'
        COMPLETED = 'completed', 'Completed'

class SocialMedia(models.TextChoices):
        INSTAGRAM = 'Instagram', 'Instagram' 
        LINKEDIN = 'LinkedIn', 'LinkedIn'
        TIKTOK = 'TikTok', 'TikTok'
        FACEBOOK = 'Facebook', 'Facebook'
        YOUTUBE = 'YouTube', 'YouTube'
    
class ContactType(models.TextChoices):
        PHONE_NUMBER = 'Phone', 'Phone'
        MAIL_ADDRESS = 'E-mail', 'E-mail'

class NGOOfferings(models.TextChoices):
        VOLUNTEERING = 'Volunteering', 'Volunteering'
        EVENTS = 'Events', 'Events' 
        DONATIONS = 'Donations', 'Donations'
        PETITIONS = 'Petitions', 'Petitions'

class BabyAnimals(models.TextChoices):
        MODERN_DRAGONS = 'Modern dragons', 'Modern dragons'
        FORESTS_DEAD_MAN = "Forest's dead man", "Forest's dead man"
        CUTIES = 'Cuties', 'Cuties'
        ALIEN_PREDATOR = 'Alien predator', 'Alien predator'

class AnimalCategory(models.TextChoices):
        HORROR = 'Horror', 'Horror'
        FANTASY = 'Fantasy', 'Fantasy'
        LEGENDARY = 'Legendary', 'Legendary'

class DevelopmentStage(models.TextChoices):
        ECO_PIONEER = 'Eco Pioneer', 'Eco Pioneer'
        BIOSPHERE_GUARDIAN ='Biosphere Guardian', 'Biosphere Guardian'
        GAIA_EMISSARY = "Gaia's Emissary", "Gaia's Emissary"
        PLANET_HEALER = "Planet Healer"
        COSMIC_STEWARD = "Cosmic Steward"