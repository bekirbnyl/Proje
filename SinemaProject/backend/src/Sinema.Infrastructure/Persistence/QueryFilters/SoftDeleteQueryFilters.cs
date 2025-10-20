using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using System.Linq.Expressions;

namespace Sinema.Infrastructure.Persistence.QueryFilters;

/// <summary>
/// Extension methods for applying soft delete query filters to entities
/// </summary>
public static class SoftDeleteQueryFilters
{
    /// <summary>
    /// Applies soft delete query filters to the model builder
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure</param>
    public static void ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to Movie
        modelBuilder.Entity<Movie>()
            .HasQueryFilter(CreateSoftDeleteFilter<Movie>());

        // Apply soft delete filter to Hall
        modelBuilder.Entity<Hall>()
            .HasQueryFilter(CreateSoftDeleteFilter<Hall>());

        // Apply soft delete filter to SeatLayout
        modelBuilder.Entity<SeatLayout>()
            .HasQueryFilter(CreateSoftDeleteFilter<SeatLayout>());

        // Apply soft delete filter to Screening
        modelBuilder.Entity<Screening>()
            .HasQueryFilter(CreateSoftDeleteFilter<Screening>());
    }

    /// <summary>
    /// Creates a soft delete filter expression for the specified entity type
    /// </summary>
    /// <typeparam name="T">Entity type that supports soft delete</typeparam>
    /// <returns>Expression for filtering out soft-deleted entities</returns>
    private static Expression<Func<T, bool>> CreateSoftDeleteFilter<T>()
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "IsDeleted");
        var condition = Expression.Equal(property, Expression.Constant(false));
        
        return Expression.Lambda<Func<T, bool>>(condition, parameter);
    }

    /// <summary>
    /// Creates a query filter that includes both soft-deleted and non-deleted entities
    /// This is used when specifically requesting to include deleted entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Expression that includes all entities regardless of deletion status</returns>
    public static Expression<Func<T, bool>> CreateIncludeDeletedFilter<T>()
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var constantTrue = Expression.Constant(true);
        
        return Expression.Lambda<Func<T, bool>>(constantTrue, parameter);
    }

    /// <summary>
    /// Extension method to temporarily ignore soft delete filters for a specific query
    /// Use this when you need to access soft-deleted entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The query to modify</param>
    /// <returns>Query that includes soft-deleted entities</returns>
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query)
        where T : class
    {
        return query.IgnoreQueryFilters();
    }

    /// <summary>
    /// Extension method to filter only soft-deleted entities
    /// Use this when you specifically want to work with deleted entities
    /// </summary>
    /// <typeparam name="T">Entity type that supports soft delete</typeparam>
    /// <param name="query">The query to modify</param>
    /// <returns>Query that only includes soft-deleted entities</returns>
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
        where T : class
    {
        return query.IgnoreQueryFilters().Where(CreateOnlyDeletedFilter<T>());
    }

    /// <summary>
    /// Creates a filter expression that only includes soft-deleted entities
    /// </summary>
    /// <typeparam name="T">Entity type that supports soft delete</typeparam>
    /// <returns>Expression for filtering only soft-deleted entities</returns>
    private static Expression<Func<T, bool>> CreateOnlyDeletedFilter<T>()
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "IsDeleted");
        var condition = Expression.Equal(property, Expression.Constant(true));
        
        return Expression.Lambda<Func<T, bool>>(condition, parameter);
    }
}
