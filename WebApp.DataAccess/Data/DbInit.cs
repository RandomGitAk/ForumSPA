using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Helpers;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Data;
public static class DbInit
{
    public static async Task InitializeAsync(IUser users, IRole roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(users);

        if (await roles.FindByNameAsync("User") == null)
        {
            await roles.AddAsync(new Role
            {
                Name = "User",
            });
        }

        if (await roles.FindByNameAsync("Moderator") == null)
        {
            await roles.AddAsync(new Role
            {
                Name = "Moderator",
            });
        }

        if (await roles.FindByNameAsync("Admin") == null)
        {
            await roles.AddAsync(new Role
            {
                Name = "Admin",
            });
        }

        var adminRoleFromDb = await roles.FindByNameAsync("Admin");
        var userRoleFromDb = await roles.FindByNameAsync("User");

        if (!await users.IsUserExistsAsync("admin@gmail.com"))
        {
            string password = "qwerty";
            string salt = SecurityHelper.GenerateSalt(70);
            string hashedPassword = SecurityHelper.HashPassword(password, salt, 10101, 70);

            User user = new User
            {
                FirstName = "Admin",
                LastName = "ForumAdmin",
                Email = "admin@gmail.com",
                Salt = salt,
                HashedPasssword = hashedPassword,
                RoleId = adminRoleFromDb!.Id,
            };

            await users.AddAsync(user);
        }

        if (!await users.IsUserExistsAsync("alex@gmail.com"))
        {
            string password = "123456789";
            string salt = SecurityHelper.GenerateSalt(70);
            string hashedPassword = SecurityHelper.HashPassword(password, salt, 10101, 70);

            User user = new User
            {
                FirstName = "User",
                LastName = "ForumUser",
                Email = "alex@gmail.com",
                Salt = salt,
                HashedPasssword = hashedPassword,
                RoleId = userRoleFromDb!.Id,
            };
            await users.AddAsync(user);
        }
    }

    public static async Task InitializeContentAsync(ApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var admin = await context.Users.FirstOrDefaultAsync(e => e.Email == "admin@gmail.com");
        var user = await context.Users.FirstOrDefaultAsync(e => e.Email == "alex@gmail.com");

        if (!await context.Categories.AnyAsync())
        {
            await context.Categories.AddRangeAsync(
             new List<Category>
                {
                        new Category
                        {
                            Name = "Technology",
                            Description = "Discussions about the latest in technology, gadgets, software, and innovations.",
                        },
                        new Category
                        {
                            Name = "Programming",
                            Description = "A place to discuss coding techniques, share code, and ask for help on various programming languages.",
                        },
                        new Category
                        {
                            Name = "Gaming",
                            Description = "Discuss video games, platforms, and game development. Share tips, reviews, and news.",
                        },
                        new Category
                        {
                            Name = "Science",
                            Description = "Explore the world of science, from physics to biology. Share discoveries, theories, and research.",
                        },
                        new Category
                        {
                            Name = "General Discussion",
                            Description = "A casual space for general conversations on any topic not covered by other categories.",
                        },
                });
            _ = await context.SaveChangesAsync();
        }

        if (!await context.Posts.AnyAsync())
        {
            await context.Posts.AddRangeAsync(
                new List<Post>
                {
                        new Post
                        {
                            Title = "Latest iPhone Release",
                            Content = "What do you think about the latest iPhone release? Is it worth the upgrade?",
                            PostedDate = DateTime.UtcNow,
                            UserId = admin!.Id,
                            Category = await context.Categories.FirstOrDefaultAsync(e => e.Name == "Technology") ?? null!,
                        },
                        new Post
                        {
                            Title = "C# Null Reference Exception Help",
                            Content = "Can anyone help me debug this C# code? I'm getting a null reference exception.",
                            PostedDate = DateTime.UtcNow,
                            UserId = user!.Id,
                            Category = await context.Categories.FirstOrDefaultAsync(e => e.Name == "Programming") ?? null!,
                        },
                        new Post
                        {
                            Title = "Baldur's Gate 3 Impressions",
                            Content = "Has anyone tried the new Baldur's Gate 3? How does it compare to Divinity: Original Sin 2?",
                            PostedDate = DateTime.UtcNow,
                            UserId = admin!.Id,
                            Category = await context.Categories.FirstOrDefaultAsync(e => e.Name == "Gaming") ?? null !,
                        },
                        new Post
                        {
                            Title = "James Webb Space Telescope Discoveries",
                            Content = "The James Webb Space Telescope has captured some amazing images of distant galaxies! Let's discuss the findings.",
                            PostedDate = DateTime.UtcNow,
                            UserId = user!.Id,
                            Category = await context.Categories.FirstOrDefaultAsync(e => e.Name == "Science") ?? null !,
                        },
                        new Post
                        {
                            Title = "Remote Work - Future or Trend?",
                            Content = "What's everyone's opinion on remote work? Is it the future or a temporary trend?",
                            PostedDate = DateTime.UtcNow,
                            UserId = admin!.Id,
                            Category = await context.Categories.FirstOrDefaultAsync(e => e.Name == "General Discussion") ?? null!,
                        },
                });

            _ = await context.SaveChangesAsync();
        }

        if (!await context.Comments.AnyAsync())
        {
            var firstPost = await context.Posts.FirstOrDefaultAsync(e => e.Title == "Latest iPhone Release") ?? null!;
            var secondPost = await context.Posts.FirstOrDefaultAsync(e => e.Title == "C# Null Reference Exception Help") ?? null!;
            var thirdPost = await context.Posts.FirstOrDefaultAsync(e => e.Title == "Baldur's Gate 3 Impressions") ?? null!;

            var initialComments = new List<Comment>
                {
                    new Comment
                    {
                        Content = "I think the new iPhone is great, but the price is a bit high for the small upgrades.",
                        Post = firstPost,
                        UserId = admin!.Id,
                    },
                    new Comment
                    {
                        Content = "Check if you're initializing all your objects properly. This error often comes from uninitialized variables.",
                        Post = secondPost,
                        UserId = user!.Id,
                    },
                    new Comment
                    {
                        Content = "Baldur's Gate 3 is amazing! The character customization and dialogue options are fantastic.",
                        Post = thirdPost,
                        UserId = admin!.Id,
                    },
                };

            await context.Comments.AddRangeAsync(initialComments);
            _ = await context.SaveChangesAsync();

            var parentComment = initialComments[0];

            var replyComment = new Comment
            {
                Content = "I agree with you! The price is really high, especially considering the minor improvements.",
                ParentCommentId = parentComment.Id,
                PostId = parentComment.PostId,
                UserId = user!.Id,
            };

            _ = await context.Comments.AddAsync(replyComment);
            _ = await context.SaveChangesAsync();
        }
    }
}
