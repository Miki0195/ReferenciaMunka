using ELTE.Cinema.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess;

public static class DbInitializer
{
    public static void Initialize(CinemaDbContext context, string imagePath, RoleManager<UserRole>? roleManager = null, UserManager<User>? userManager = null)
    {
        if (!Path.Exists(imagePath))
            throw new IOException("Image path does not exists");

        context.Database.Migrate();

        if (roleManager != null)
        {
            SeedRolesAsync(roleManager).Wait();
        }

        if (userManager != null)
        {
            SeedUsersAsync(userManager).Wait();
        }


        // Check if any movies already exist
        if (context.Movies.Any())
        {
            return;
        }

        // Movies
        Movie[] movies =
        [
            new Movie
            {
                Title = "Inception",
                Year = 2010,
                Director = "Christopher Nolan",
                Synopsis =
                    "Dom Cobb is a skilled thief specializing in extracting secrets from within the subconscious during the dream state. When a powerful businessman offers Cobb a chance to clear his criminal record, he is tasked with a near-impossible mission: inception. Instead of stealing an idea, Cobb and his team must plant one deep within the target's mind. As they descend through complex dream layers, Cobb faces a powerful adversary rooted in his own troubled past.",
                Length = 148,
                CreatedAt = DateTime.Now.AddDays(-1),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "Inception.jpg")),
            },
            new Movie
            {
                Title = "Interstellar",
                Year = 2014,
                Director = "Christopher Nolan",
                Synopsis =
                    "In a dystopian future where Earth faces environmental collapse, former NASA pilot Cooper joins a team of astronauts on an intergalactic journey to find a habitable planet for humanity. Leaving his family behind, Cooper ventures through wormholes and distant galaxies, confronting the mysteries of space, time, and gravity. As the mission grows increasingly perilous, he must choose between family and the survival of the human race.",
                Length = 169,
                CreatedAt = DateTime.Now.AddDays(-2),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "Interstellar.jpg")),
            },
            new Movie
            {
                Title = "2001: A Space Odyssey",
                Year = 1968,
                Director = "Stanley Kubrick",
                Synopsis =
                    "An epic voyage through human evolution and space exploration, this landmark sci-fi classic follows Dr. Dave Bowman and his team as they journey to Jupiter with the advanced AI computer, HAL 9000. When HAL begins to display erratic behavior, the astronauts' lives are jeopardized, leading to a confrontation that questions the boundaries between man and machine.",
                Length = 149,
                CreatedAt = DateTime.Now,
                Image = File.ReadAllBytes(Path.Combine(imagePath, "2001ASpaceOdyssey.jpg")),
            },
            new Movie
            {
                Title = "Alien",
                Year = 1979,
                Director = "Ridley Scott",
                Synopsis =
                    "In the depths of space, the crew of the Nostromo responds to a distress signal on an unexplored planet. They soon discover a terrifying alien life form with deadly capabilities. When one of the crew members is infected, they become locked in a battle for survival, facing a relentless creature that threatens to kill them all. This sci-fi horror classic explores themes of isolation and the unknown.",
                Length = 117,
                CreatedAt = DateTime.Now.AddDays(-5),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "Alien.jpg")),
            },
            new Movie
            {
                Title = "Star Wars: Episode IV - A New Hope",
                Year = 1977,
                Director = "George Lucas",
                Synopsis =
                    "In a galaxy oppressed by the dark forces of the Empire, young Luke Skywalker embarks on a heroic adventure after discovering a message hidden in a droid. Guided by the wise Jedi Obi-Wan Kenobi, Luke joins forces with Han Solo and Princess Leia to save the galaxy and destroy the Empire’s superweapon, the Death Star. This groundbreaking film marked the beginning of a new era in science fiction.",
                Length = 121,
                CreatedAt = DateTime.Now.AddDays(-3),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "StarWars4.jpg")),
            },
            new Movie
            {
                Title = "Star Wars: Episode V - The Empire Strikes Back",
                Year = 1980,
                Director = "Irvin Kershner",
                Synopsis =
                    "In this darker chapter of the Star Wars saga, the Rebel Alliance faces significant setbacks as they are pursued by Darth Vader and the Imperial fleet. Meanwhile, Luke Skywalker travels to the distant planet of Dagobah to train with the wise Jedi Master Yoda. As Luke grows stronger in the Force, he must confront shocking revelations about his heritage and face his most formidable enemy.",
                Length = 124,
                CreatedAt = DateTime.Now.AddDays(-4),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "StarWars5.jpg")),
            },
            new Movie
            {
                Title = "Star Wars: Episode VI - Return of the Jedi",
                Year = 1983,
                Director = "Richard Marquand",
                Synopsis =
                    "The Rebel Alliance mounts a final assault on the Empire in an attempt to overthrow the Emperor and bring peace to the galaxy. Luke Skywalker, now a powerful Jedi Knight, confronts Darth Vader in a climactic duel that will determine the fate of his friends and the entire galaxy. With the help of his allies, Luke seeks to end the tyranny of the Dark Side once and for all.",
                Length = 131,
                CreatedAt = DateTime.Now.AddDays(-4),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "StarWars6.jpg")),
            },
            new Movie
            {
                Title = "The Matrix",
                Year = 1999,
                Director = "Lana Wachowski, Lilly Wachowski",
                Synopsis =
                    "In a dystopian world controlled by machines, computer hacker Neo discovers a shocking truth: reality is a simulated construct designed to enslave humanity. Guided by the mysterious Morpheus and the skilled warrior Trinity, Neo learns to break free from the Matrix and fights to liberate mankind. This film redefined action sci-fi with its mind-bending plot and revolutionary visual effects.",
                Length = 136,
                CreatedAt = DateTime.Now.AddDays(-4),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "TheMatrix1.jpg")),
            },
            new Movie
            {
                Title = "The Matrix Reloaded",
                Year = 2003,
                Director = "Lana Wachowski, Lilly Wachowski",
                Synopsis =
                    "Neo, now a seasoned freedom fighter, battles new dangers within and beyond the Matrix. As he faces increasingly powerful adversaries and uncovers secrets about the human resistance, Neo must contend with a growing threat to humanity and the possibility of an imminent war against the machines. Filled with spectacular action sequences, this sequel expands the mythos of the Matrix universe.",
                Length = 138,
                CreatedAt = DateTime.Now.AddDays(-5),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "TheMatrix2.jpg")),
            },
            new Movie
            {
                Title = "The Matrix Revolutions",
                Year = 2003,
                Director = "Lana Wachowski, Lilly Wachowski",
                Synopsis =
                    "In the explosive finale to the Matrix trilogy, Neo and his allies face their most daunting battle as they fight to protect humanity from annihilation. With the city of Zion under siege, Neo’s journey leads him to a confrontation with the powerful Architect of the Matrix. A climactic showdown unfolds, determining the fate of both the real and digital worlds.",
                Length = 129,
                CreatedAt = DateTime.Now.AddDays(-5),
                Image = File.ReadAllBytes(Path.Combine(imagePath, "TheMatrix3.jpg")),
            },
        ];

        context.Movies.AddRange(movies);

        // Rooms
        Room[] rooms =
        [
            new Room { Name = "Room 1", Rows = 10, Columns = 20 }, // Room 1
            new Room { Name = "Room 2", Rows = 15, Columns = 24 }, // Room 2
            new Room { Name = "Room 3", Rows = 8, Columns = 12 } // Room 3
        ];

        context.Rooms.AddRange(rooms);

        // Screenings
        var screenings = new List<Screening>();

        for (int delayDays = 0; delayDays < 7; ++delayDays)
        {
            int delayMinutes = 0;
            foreach (var movie in movies)
            {
                screenings.Add(new Screening
                {
                    Movie = movie,
                    Room = rooms[0], // Assign to Room 1
                    StartsAt = DateTime.Today.AddDays(delayDays).AddHours(10).AddMinutes(delayMinutes),
                    Price = 10.00m
                });

                delayMinutes += 190;
            }

            delayMinutes = 0;
            foreach (var movie in movies.Reverse())
            {
                screenings.Add(new Screening
                {
                    Movie = movie,
                    Room = rooms[1], // Assign to Room 2
                    StartsAt = DateTime.Today.AddDays(delayDays).AddHours(11).AddMinutes(delayMinutes),
                    Price = 12.00m
                });

                delayMinutes += 190;
            }
        }

        context.Screenings.AddRange(screenings);

        // Save changes to the database
        context.SaveChanges();
    }

    private static async Task SeedRolesAsync(RoleManager<UserRole> roleManager)
    {
        string[] roleNames = ["Admin"];

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                await roleManager.CreateAsync(new UserRole(roleName));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        // Example to seed an Admin user
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User { UserName = adminEmail, Email = adminEmail, Name = "Test Admin", RefreshToken = Guid.NewGuid() };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}