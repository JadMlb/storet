using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.StorageLocations;

namespace Storet.Modules.Inventory.Services.StorageLocations;

public class StorageLocationsService : IStorageLocationsService
{
	private readonly IStorageLocationsRepository storageLocationsRepository;
	private readonly ICurrentUser user;
	private readonly IMapper mapper;
	
	public StorageLocationsService (IStorageLocationsRepository storageLocationsRepository, ICurrentUser user, IMapper mapper)
	{
		this.storageLocationsRepository = storageLocationsRepository;
		this.user = user;
		this.mapper = mapper;
	}
	
	public async Task<IEnumerable<StorageLocationResponse>> GetAllAsync ()
	{
		var locations = await storageLocationsRepository.GetAllAsync (user.Id);
		return locations.Select (mapper.Map<StorageLocation, StorageLocationResponse>);
	}
	
	public async Task<StorageLocationDetailsResponse?> GetOneAsync (Guid key)
	{
		var location = await storageLocationsRepository.GetOneAsync (key, user.Id);
		if (location == null)
			return null;
			
		return mapper.Map<StorageLocation, StorageLocationDetailsResponse> (location);
	}
	
	public async Task<StorageLocationDetailsResponse?> InsertAsync (StorageLocationInsertRequest model)
	{
		if (string.IsNullOrWhiteSpace (model.Name))
			throw new ArgumentException ("Name cannot be empty");
		
		try
		{
			var result = await storageLocationsRepository.InsertAsync (mapper.Map<StorageLocationInsertRequest, StorageLocation> (model));
			if (result == null)
				return null;
			return mapper.Map<StorageLocation, StorageLocationDetailsResponse> (result);
		}
		catch (DbUpdateException)
		{
			throw new DuplicateKeyException (nameof (StorageLocation), model.Name);
		}
		catch (Exception)
		{
			return null;
		}
	}
	
	public async Task<StorageLocationDetailsResponse?> UpdateAsync (Guid key, StorageLocationUpdateRequest model)
	{
		if (string.IsNullOrWhiteSpace (model.Name))
			throw new ArgumentException ("Name cannot be empty");
			
		var existing = await storageLocationsRepository.GetOneAsync (key, user.Id);
		if (existing == null)
			return null;
		
		try
		{
			var result = await storageLocationsRepository.UpdateAsync (key, user.Id, mapper.Map<StorageLocationUpdateRequest, StorageLocation> (model));
			return mapper.Map<StorageLocation, StorageLocationDetailsResponse> (result!);
		}
		catch (DbUpdateException)
		{
			throw new DuplicateKeyException (nameof (StorageLocation), model.Name);
		}
		catch (Exception)
		{
			return null;
		}
	}
	
	public async Task<bool> DeleteAsync (Guid key)
	{
		try
		{
			return await storageLocationsRepository.DeleteAsync (key, user.Id);
		}
		catch (DbUpdateException)
		{
			throw new EntityDependencyException (nameof (StorageLocation), key);
		}
		catch (Exception)
		{
			return false;
		}
	}
}