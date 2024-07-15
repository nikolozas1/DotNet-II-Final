using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using Reddit.Repositories;

namespace Reddit.UnitTest;

public class UnitTest1
{

    private IPostsRepository GetPostsRepostory()
    {

        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var dbContext = new ApplicationDbContext(options);

        dbContext.Posts.Add(new Post { Title = "my kitty, tehee", Content = "*picture of an obese dying cat*", Upvotes = 5712001, Downvotes = 1 });
        dbContext.Posts.Add(new Post { Title = "Is it okay to eat chicken from sewers?", Content = "It's got natural seasonings", Upvotes = 109, Downvotes = 14 });
        dbContext.Posts.Add(new Post { Title = "GUYS I JUST FOUND CURE TO CANCER!!!", Content = "*post was banned for unkown reaseons*", Upvotes = 3, Downvotes = 1 });
        dbContext.Posts.Add(new Post { Title = "Just got promoted to manager", Content = "In blockbuster", Upvotes = 99, Downvotes = 523 });
        dbContext.Posts.Add(new Post { Title = "le challenge", Content = "guyz make downvotes and upvotes equal pliz", Upvotes = 224, Downvotes = 224 });
        dbContext.Posts.Add(new Post { Title = "Title 6", Content = "too lazy to write", Upvotes = 3000, Downvotes = 10 });
        dbContext.Posts.Add(new Post { Title = "My opinion", Content = "I think women deserve rights", Upvotes = 5, Downvotes = 10000000 });
        dbContext.Posts.Add(new Post { Title = "Improtant announcement", Content = "I love boobs <3 <3 <3", Upvotes = 94, Downvotes = 21 });
        dbContext.SaveChanges();

        return new PostsRepository(dbContext);
    }

    [Fact]
    public async Task GetPosts_ReturnsCorrectPagination()
    {
        var postsRepository = GetPostsRepostory();
        var posts = await postsRepository.GetPosts(2, 4, null, null, null);

        Assert.Equal(4, posts.Items.Count);
        Assert.Equal(8, posts.TotalCount);
        Assert.False(posts.HasNextPage);
        Assert.True(posts.HasPreviousPage);
    }


    [Fact]
    public async Task GetPosts_SortPopularCorrect()
    {
        var postsRepository = GetPostsRepostory();
        var posts = await postsRepository.GetPosts(1, 4, null, "popular", false);

        Assert.Equal(4, posts.Items.Count);
        Assert.Equal(8, posts.TotalCount);
        Assert.True(posts.HasNextPage);
        Assert.False(posts.HasPreviousPage);
        Assert.Equal("My opinion", posts.Items.First().Title);
    }

    [Fact]
    public async Task GetPosts_SortPositiveCorrect()
    {
        var postsRepository = GetPostsRepostory();
        var posts = await postsRepository.GetPosts(2, 2, null, "positivity", false);

        Assert.True(posts.HasNextPage);
        Assert.True(posts.HasPreviousPage);
        Assert.Equal("GUYS I JUST FOUND CURE TO CANCER!!!", posts.Items.First().Title);
    }

    [Fact]
    public async Task GetPosts_SearchTermCorrect()
    {
        var postsRepository = GetPostsRepostory();
        var posts = await postsRepository.GetPosts(page: 1, pageSize: 8, searchTerm: "boobs", SortTerm: null);
        Assert.Single(posts.Items);
        Assert.Equal("Improtant announcement", posts.Items.First().Title);
    }

    [Fact]
    public async Task GetPosts_InvalidPage_ThrowsArgumentException()
    {
        var repository = GetPostsRepostory();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPosts(page: 0, pageSize: 10, searchTerm: null, SortTerm: null));
        Assert.Equal("page", exception.ParamName);
    }

    [Fact]
    public async Task GetPosts_InvalidPageSize_ThrowsArgumentOutOfRangeException()
    {
        var repository = GetPostsRepostory();

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPosts(page: 1, pageSize: 0, searchTerm: null, SortTerm: null));
        Assert.Equal("pageSize", exception.ParamName);
    }
}