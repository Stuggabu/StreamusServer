﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Streamus_Web_API.Controllers;
using Streamus_Web_API.Domain;
using Streamus_Web_API.Domain.Interfaces;
using Streamus_Web_API.Dto;

namespace Streamus_Web_API_Tests.Controller_Tests
{
  [TestFixture]
  public class PlaylistItemControllerTest : StreamusTest
  {
    private PlaylistItemController _playlistItemController;
    private IPlaylistManager _playlistManager;
    private IPlaylistItemManager _playlistItemManager;

    [SetUp]
    public new void TestFixtureSetUp()
    {
      _playlistItemController = new PlaylistItemController(Logger, Session, ManagerFactory);
      _playlistManager = ManagerFactory.GetPlaylistManager(Session);
      _playlistItemManager = ManagerFactory.GetPlaylistItemManager(Session);
    }

    [Test]
    public void CreatePlaylistItem_PlaylistItemDoesntExist_PlaylistItemCreated()
    {
      PlaylistItemDto playlistItemDto = Helpers.CreatePlaylistItemDto();

      var createdPlaylistItemDto = _playlistItemController.Create(playlistItemDto);

      //  Make sure we actually get a PlaylistItem DTO back from the Controller.
      Assert.NotNull(createdPlaylistItemDto);

      Playlist playlist = _playlistManager.Get(createdPlaylistItemDto.PlaylistId);

      //  Make sure that the created playlistItem was cascade added to the Playlist
      Assert.That(playlist.Items.Count(i => i.Id == createdPlaylistItemDto.Id) == 1);
    }

    /// <summary>
    /// 50 PlaylistItems is the largest chunk expected to be saved in one burst because
    /// the YouTube API maxes out at 50 return items.
    /// </summary>
    [Test]
    public void Create50PlaylistItems_PlaylistEmpty_AllItemsCreated()
    {
      const int numItemsToCreate = 50;
      List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate);

      var createdPlaylistItemDtos = _playlistItemController.CreateMultiple(playlistItemDtos);

      //  Make sure we actually get the list back from the Controller.
      Assert.NotNull(createdPlaylistItemDtos);
      Assert.That(createdPlaylistItemDtos.Count() == numItemsToCreate);

      Playlist playlist = _playlistManager.Get(playlistItemDtos.First().PlaylistId);

      //  Make sure that the created playlistItem was cascade added to the Playlist
      Assert.That(playlist.Items.Count == numItemsToCreate);
    }

    /// <summary>
    /// A StackOverflowException will occur if mappings don't contain fetch="join." Ensure this rule is in place.
    /// </summary>
    [Test]
    public void CreatePlaylistItemsRepeatedly_PlaylistEmpty_NoStackOverflowException()
    {
      const int iterations = 2;
      const int numItemsToCreate = 2142;
      Guid playlistId = default(Guid);

      //  Starting at 1 because I want to use currentIteration to be used in math and makes more sense as 1.
      for (int currentIteration = 1; currentIteration <= iterations; currentIteration++)
      {
        List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate, playlistId);

        var createdPlaylistItemDtos = _playlistItemController.CreateMultiple(playlistItemDtos);

        //  Make sure we actually get the list back from the Controller.
        Assert.NotNull(createdPlaylistItemDtos);
        Assert.That(createdPlaylistItemDtos.Count() == numItemsToCreate);

        Playlist playlist = _playlistManager.Get(playlistItemDtos.First().PlaylistId);
        playlistId = playlist.Id;

        //  Make sure that the created playlistItem was cascade added to the Playlist
        Assert.That(playlist.Items.Count == numItemsToCreate * currentIteration);
      }
    }

    [Test]
    public void CreateMaxPlaylistItems_PlaylistEmpty_DoesntTakeForFuckingEver()
    {
      const int iterations = 1;
      const int numItemsToCreate = 1;
      Guid playlistId = default(Guid);

      //  Starting at 1 because I want to use currentIteration to be used in math and makes more sense as 1.
      for (int currentIteration = 1; currentIteration <= iterations; currentIteration++)
      {
        List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate, playlistId);

        var createdPlaylistItemDtos = _playlistItemController.CreateMultiple(playlistItemDtos);

        //  Make sure we actually get the list back from the Controller.
        Assert.NotNull(createdPlaylistItemDtos);
        Assert.That(createdPlaylistItemDtos.Count() == numItemsToCreate);

        Playlist playlist = _playlistManager.Get(playlistItemDtos.First().PlaylistId);
        playlistId = playlist.Id;

        //  Make sure that the created playlistItem was cascade added to the Playlist
        Assert.That(playlist.Items.Count == numItemsToCreate * currentIteration);
      }
    }

    [Test]
    public void DeletePlaylistItem_ItemExistsInPlaylist_DeletedSuccessfully()
    {
      PlaylistItemDto playlistItemDto = Helpers.CreatePlaylistItemDto();

      var createdPlaylistItemDto = _playlistItemController.Create(playlistItemDto);

      //  Make sure we actually get a PlaylistItem DTO back from the Controller.
      Assert.NotNull(createdPlaylistItemDto);

      Playlist playlist = _playlistManager.Get(createdPlaylistItemDto.PlaylistId);

      //  Make sure that the created playlistItem was cascade added to the Playlist
      Assert.That(playlist.Items.Count(i => i.Id == createdPlaylistItemDto.Id) == 1);

      _playlistItemController.Delete(createdPlaylistItemDto.Id);
    }

    [Test]
    public void PatchPlaylistItem_SequenceProvided_SequenceModified()
    {
      PlaylistItemDto playlistItemDto = Helpers.CreatePlaylistItemDto();
      var createdPlaylistItemDto = _playlistItemController.Create(playlistItemDto);

      const double newSequence = 5;

      PlaylistItemDto patchedPlaylistItemDto = new PlaylistItemDto { Sequence = newSequence };

      PlaylistItem playlistItem = _playlistItemManager.Get(createdPlaylistItemDto.Id);

      _playlistItemController.Patch(playlistItem.Id, patchedPlaylistItemDto);

      Assert.AreEqual(playlistItem.Sequence, newSequence);
    }
  }
}