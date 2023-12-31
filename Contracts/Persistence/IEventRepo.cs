﻿using BasicLayeredService.API.Domain;
using BasicLayeredService.API.DTOs;

namespace BasicLayeredService.API.Contracts.Persistence;

public interface IEventRepo : IBaseItemRepo<Event>
{
    Task<QueryResult<List<Event>>> QueryAsync(QueryDto dto);
}
