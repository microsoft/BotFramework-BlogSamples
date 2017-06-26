// Copyright (c) Microsoft Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace TriviaBot.Runtime
{
    /// <summary>
    /// This list of categories comes from the list of supported categories on https://opentdb.com/
    /// </summary>
    public enum TriviaCategory
    {
        None = -1,

        [Display(Name = "Any")]
        Any = 0,

        [Display(Name = "General Knowledge")]
        GeneralKnowledge = 9,

        [Display(Name = "Entertainment: Books")]
        EntertainmentBooks,

        [Display(Name = "Entertainment: Film")]
        EntertainmentFilm,

        [Display(Name = "Entertainment: Music")]
        EntertainmentMusic,

        [Display(Name = "Entertainment: Musicals & Theatres")]
        EntertainmentMusicalsTheatre,

        [Display(Name = "Entertainment: Television")]
        EntertainmentTelevision,

        [Display(Name = "Entertainment: Video Games")]
        EntertainmentVideoGames,

        [Display(Name = "Entertainment: Board Games")]
        EntertainmentBoardGames,

        [Display(Name = "Science & Nature")]
        ScienceNature,

        [Display(Name = "Science: Computers")]
        ScienceComputers,

        [Display(Name = "Science: Mathematics")]
        ScienceMathematics,

        [Display(Name = "Mythology")]
        Mythology,

        [Display(Name = "Sports")]
        Sports,

        [Display(Name = "Geography")]
        Geography,

        [Display(Name = "History")]
        History,

        [Display(Name = "Politics")]
        Politics,

        [Display(Name = "Art")]
        Art,

        [Display(Name = "Celebrities")]
        Celebrities,

        [Display(Name = "Animals")]
        Animals,

        [Display(Name = "Vehicles")]
        Vehicles,

        [Display(Name = "Entertainment: Comics")]
        EntertainmentComics,

        [Display(Name = "Science: Gadgets")]
        ScienceGadgets,

        [Display(Name = "Entertainment: Japanese Anime & Manga")]
        EntertainmentJapaneseAnimeManga,
    }
}