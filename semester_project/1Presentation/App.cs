using semester_project._2Application.Interfaces;
using semester_project._2Application.UseCases.Favorites;
using semester_project._2Application.UseCases.Leaderboard;
using semester_project._2Application.UseCases.Media;
using semester_project._2Application.UseCases.Ratings;
using semester_project._2Application.UseCases.Recommendations;
using semester_project._2Application.UseCases.Users;
using semester_project._4Infrastructure;
using semester_project._4Infrastructure.data;
using semester_project._4Infrastructure.repositories;

namespace semester_project._1Presentation;

public static class App
{
    public static RegisterUserHandler RegisterUser { get; private set; } = null!;
    public static LoginUserHandler LoginUser { get; private set; } = null!;
    public static GetUserProfileHandler GetUserProfile { get; private set; } = null!;
    public static UpdateUserProfileHandler UpdateUserProfile { get; private set; } = null!;
    public static GetUserByUsernameHandler GetUserByUsername { get; private set; } = null!;
    public static ITokenService Tokens { get; private set; } = null!; // expose token service
    public static CreateMediaHandler CreateMedia { get; private set; } = null!;
    public static DeleteMediaHandler DeleteMedia { get; private set; } = null!;
    public static UpdateMediaHandler UpdateMedia { get; private set; } = null!;

    public static FavoriteMediaHandler FavoriteMedia { get; private set; } = null!;
    public static UnfavoriteMediaHandler UnfavoriteMedia { get; private set; } = null!;

    public static CreateRatingHandler CreateRate { get; private set; } = null!;
    public static UpdateRatingHandler UpdateRate { get; private set; } = null!;
    public static DeleteRatingHandler DeleteRate { get; private set; } = null!;
    public static ApproveRatingCommentHandler ConfirmRate { get; private set; } = null!;
    public static LikeRateHandler LikeRate { get; private set; } = null!;

    public static GetUserRatingHistoryHandler GetUserRatingHistory { get; private set; } = null!;
    public static GetUserFavoritesHandler GetUserFavorites { get; private set; } = null!;

    public static GetUserRecommendationsHandler GetUserRecommendations { get; private set; } = null!;

    public static GetLeaderboardHandler GetLeaderboard { get; private set; } = null!;

    public static SearchMediaHandler SearchMedia { get; private set; } = null!;
    
    public static GetMediaByIdHandler GetMediaById { get; private set; } = null!;


    public static void Configure()
    {
        var connStr =
            Environment.GetEnvironmentVariable("MRP_DB") ??
            "Host=localhost;Port=5432;Database=mrp;Username=mrp_user;Password=mrp_password";

        var factory = new PostgresConnectionFactory(connStr);
        IUserRepository usersRepo = new PostgresUserRepository(factory);
        ITokenService tokenService = new StaticTokenService();
        IMediaRepository mediaRepo = new PostgresMediaRepository(factory);
        IRatingRepository ratingRepo = new PostgresRatingRepository(factory);
        IFavoriteRepository favoriteRepo = new PostgresFavoriteRepository(factory);
        IRecommendationRepository recommendationRepo = new PostgresRecommendationRepository(factory);
        ILeaderboardRepository leaderboardRepo = new PostgresLeaderboardRepository(factory);

        RegisterUser = new RegisterUserHandler(usersRepo, tokenService);
        LoginUser = new LoginUserHandler(usersRepo, tokenService);
        GetUserProfile = new GetUserProfileHandler(usersRepo);
        UpdateUserProfile = new UpdateUserProfileHandler(usersRepo);
        GetUserByUsername = new GetUserByUsernameHandler(usersRepo);
        Tokens = tokenService;
        CreateMedia = new CreateMediaHandler(mediaRepo);
        DeleteMedia = new DeleteMediaHandler(mediaRepo);
        UpdateMedia = new UpdateMediaHandler(mediaRepo);

        FavoriteMedia = new FavoriteMediaHandler(mediaRepo);
        UnfavoriteMedia = new UnfavoriteMediaHandler(mediaRepo);

        CreateRate = new CreateRatingHandler(ratingRepo, mediaRepo);
        UpdateRate = new UpdateRatingHandler(ratingRepo);
        DeleteRate = new DeleteRatingHandler(ratingRepo);
        ConfirmRate = new ApproveRatingCommentHandler(ratingRepo);
        LikeRate = new LikeRateHandler(ratingRepo);

        GetUserRatingHistory = new GetUserRatingHistoryHandler(ratingRepo);
        GetUserFavorites = new GetUserFavoritesHandler(favoriteRepo);

        GetUserRecommendations = new GetUserRecommendationsHandler(recommendationRepo);

        GetLeaderboard = new GetLeaderboardHandler(leaderboardRepo);
        SearchMedia = new SearchMediaHandler(mediaRepo);
        
        GetMediaById = new GetMediaByIdHandler(mediaRepo);
    }
}