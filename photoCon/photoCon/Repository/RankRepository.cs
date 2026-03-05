using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;
using photoCon.Models;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;

namespace photoCon.Repository
{
    public class RankRepository : IRankRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;
        public RankRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public List<MyRankScore> GetMyRankScores(string date, int dayNumber, bool imIsActive, string round, string userId, int regionId, int categoryId)
        {
            List<string> x_ = new List<string>();
            x_.Add(userId);
            var xAnonymous = x_.Select(id => new { judgeId = id }).ToList();

            List<JudgeDDay> tempTableA = new List<JudgeDDay>();
            List<JudgeDDay> tempTableB = new List<JudgeDDay>();
            foreach (var item in xAnonymous)
            {
                tempTableB = (from a in _tallyProgramContext.ImageBatches
                              join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                              from joinab in ab.DefaultIfEmpty()
                              where a.Date == date
                              && a.DayNumber == dayNumber
                              && a.IsActive == true
                              && joinab.RegionId == regionId
                              && joinab.CategoryId == categoryId
                              select new JudgeDDay
                              {
                                  judgeId = item.judgeId,
                                  photoId = a.ImageHashId,
                                  Round = round,

                              }).ToList();

                tempTableA.AddRange(tempTableB);


            }

            tempTableA = (from a in tempTableA
                          where a.judgeId == userId
                          select a).ToList();



            var myList = (
                from x in tempTableA
                join a in _tallyProgramContext.ImageScore
                     on new { x1 = x.photoId, x2 = x.judgeId, x3 = x.Round } equals new { x1 = a.PhotoId, x2 = a.UserId , x3 = a.Round }  into xa
                from joinxa in xa.DefaultIfEmpty()
                join b in _tallyProgramContext.ImageBatches
                    on x.photoId equals b.ImageHashId into xb
                from joinxb in xb.DefaultIfEmpty()
                join c in _tallyProgramContext.PhotoLocations
                    on x.photoId equals c.HashPhotoId into xc
                from joinxc in xc.DefaultIfEmpty()
                join d in _tallyProgramContext.PhotoMetaData
                    on x.photoId equals d.HashPhotoId into xd
                from joinxd in xd.DefaultIfEmpty()
                join e in _tallyProgramContext.TblRegions on joinxc.RegionId equals e.RegionId into xe
                from joinxe in xe.DefaultIfEmpty()
                join f in _tallyProgramContext.Categories on joinxc.CategoryId equals f.CategoryId into xf
                from joinxf in xf.DefaultIfEmpty()
                join g in _tallyProgramContext.CSV_PhotoDetails on x.photoId equals g.photohash into xg
                from joinxg in xg.DefaultIfEmpty()
                where
                    joinxb.Date == date
                    && joinxb.DayNumber == dayNumber
                    && joinxb.IsActive == imIsActive
                    && joinxc.RegionId == regionId
                    && joinxc.CategoryId == categoryId
                   
                select new MyRankScore
                {
                    ImageUrl = joinxd.ImageUrl,
                    ImageSort = joinxb.Sort,
                    HashPhotoId = x.photoId,
                    MyScore = joinxa != null ? joinxa.Score : 0.00m,
                    RegionId = joinxc.RegionId,
                    RegionName = joinxe.RegionName,
                    CategoryId = joinxc.CategoryId,
                    CategoryName = joinxf.CategoryName,
                    Round = joinxa != null ? joinxa.Round : round,
                    IsUnranked = joinxa == null ? true : false,
                    Location = (joinxg != null && joinxg.location != null ) ? joinxg.location : "Not Available",
                    Description = (joinxg != null && joinxg.photodesc != null) ? joinxg.photodesc : "Not Available",
                    PhotoTitle = (joinxg != null && joinxg.phototitle != null) ? joinxg.phototitle : "Not Available",
                    PhotoTaken = (joinxg != null && joinxg.datetaken != null) ? joinxg.datetaken : "Not Available",

                }
            ).OrderByDescending(x => x.MyScore).ToList();

            return myList.ToList();
        }

        public List<OverallPhotoRankView> GetOverallRankingView(string date, int dayNumber,  string round,  int regionId, int categoryId)
        {
            //int take = _tallyProgramContext.ImageBatches.Where(x => x.Date == date && x.DayNumber == dayNumber).Count();


            ////get the active round 
            //var activeRound = _tallyProgramContext
            //                        .ReferenceCode
            //                        .Where(x => 
            //                                    (x.RefTypeID == "05" || x.RefTypeID == "06")
            //                                    && x.IsActive == 1
            //                                )
            //                        .Select(x => (x.RefTypeID.ToString() + x.RefCodeID.ToString()))
            //                        .FirstOrDefault();

            //if (round == "0502" && round == activeRound )
            //{
            //    //take
            //    take = int.Parse(_tallyProgramContext.ProcessParameters
            //                                .Where(x => x.ParameterValue == date
            //                                && x.Filler01 == dayNumber.ToString()
            //                                && x.IsActive == 1)
            //                                .Select(x => x.Filler03).FirstOrDefault());
            //}






            ////tempTable ----driver table
            //var TempA = (from a in _tallyProgramContext.ImageBatches
            //             join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
            //             from joinab in ab.DefaultIfEmpty()
            //             join c in _tallyProgramContext.ImageScore on a.ImageHashId equals c.PhotoId into ac
            //             from joinac in ac.DefaultIfEmpty()
            //             where joinab.RegionId == regionId && joinab.CategoryId == categoryId && joinab.PhotoCode == "10"
            //             && a.Date == date && a.DayNumber == dayNumber
            //             group new { joinab, joinac } by new { joinab.HashPhotoId, joinab.RegionId, joinab.CategoryId, joinac.Round } into z
            //             select new DriverRankTable
            //             {
            //                 photoId = z.Key.HashPhotoId,
            //                 regionId = z.Key.RegionId,
            //                 categoryId = z.Key.CategoryId,
            //                 round = round,
            //                 AveScore = z.Average(x => x.joinac != null && x.joinac.Score != default ? x.joinac.Score : 0.00m)

            //             }).ToList().OrderByDescending(x => x.AveScore).Take(take);
            ////var TempA = (from a in _tallyProgramContext.ImageBatches
            ////             join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
            ////             from joinab in ab.DefaultIfEmpty()
            ////             join c in _tallyProgramContext.ImageScore on a.ImageHashId  equals c.PhotoId into ac
            ////             from joinac in ac.DefaultIfEmpty()
            ////             where joinab.RegionId == regionId && joinab.CategoryId == categoryId && joinab.PhotoCode == "10"
            ////             && a.Date == date && a.DayNumber == dayNumber
            ////             select new DriverRankTable
            ////             {
            ////                 photoId = joinab.HashPhotoId,
            ////                 regionId = joinab.RegionId,
            ////                 categoryId = joinab.CategoryId,
            ////                 round = (joinac != null && joinac.Round == round) ? joinac.Round : round,

            ////             }).ToList();
            ///
            //get the average and other information
            var getAverage = (from a in _tallyProgramContext.PhotoLocations
                              join b in _tallyProgramContext.ImageBatches on a.HashPhotoId equals b.ImageHashId into ab
                              from b in ab.DefaultIfEmpty()
                              join c in _tallyProgramContext.ImageScore on a.HashPhotoId equals c.PhotoId into ac
                              from c in ac.DefaultIfEmpty()
                              join d in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals d.HashPhotoId into ad
                              from d in ad.DefaultIfEmpty()
                              join e in _tallyProgramContext.TblRegions on a.RegionId equals e.RegionId into ae
                              from e in ae.DefaultIfEmpty()
                              join f in _tallyProgramContext.Categories on a.CategoryId equals f.CategoryId into af
                              from f in af.DefaultIfEmpty()
                              join h in _tallyProgramContext.CSV_PhotoDetails on d.FileName equals h.photoname into ah
                              from joinah in ah.DefaultIfEmpty()
                              where a.RegionId == regionId && a.CategoryId == categoryId && a.PhotoCode == "10"
                              && c.Round == round && b.Date == date && b.DayNumber == dayNumber
                              group new { a, b, c, d, e, f , joinah } by new { a.HashPhotoId, a.RegionId, a.CategoryId, d.ImageUrl, e.RegionName, f.CategoryName, joinah.location , joinah.photodesc , joinah.phototitle, joinah.datetaken } into g
                              select new
                              {
                                  ImageUrl = g.Key.ImageUrl,
                                  HashPhotoId = g.Key.HashPhotoId,
                                  RegionId = g.Key.RegionId,
                                  CategoryId = g.Key.CategoryId,
                                  RegionName = g.Key.RegionName,
                                  CategoryName = g.Key.CategoryName,
                                  Location = g.Key.location,
                                  Description = g.Key.photodesc,
                                  PhotoTitle = g.Key.phototitle,
                                  PhotoTaken = g.Key.datetaken,
                                  AverageScore = g.Average(x => x.c.Score == null ? 0.00m : x.c.Score)
                              }).ToList().OrderByDescending(x => x.AverageScore == null ? 0.00m : x.AverageScore);


            ////get the average and other information on new { x1 = x.photoId, x2 = x.judgeId, x3 = x.Round } equals new { x1 = a.PhotoId, x2 = a.UserId , x3 = a.Round }  into xa
            //var getAverage = (from x in TempA
            //                  join a in _tallyProgramContext.ImageScore
            //                        on new { x1 = x.photoId, x2 = x.round } equals new { x1 = a.PhotoId, x2 = a.Round } into xa
            //                  from joinxa in xa.DefaultIfEmpty()
            //                  join b in _tallyProgramContext.PhotoMetaData on x.photoId equals b.HashPhotoId into xb
            //                  from joinxb in xb.DefaultIfEmpty()
            //                  join c in _tallyProgramContext.TblRegions on x.regionId equals c.RegionId into xc
            //                  from joinxc in xc.DefaultIfEmpty()
            //                  join d in _tallyProgramContext.Categories on x.categoryId equals d.CategoryId into xd
            //                  from joinxd in xd.DefaultIfEmpty()
            //                  join e in _tallyProgramContext.CSV_PhotoDetails on x.photoId equals e.photohash into xe
            //                  from joinxe in xe.DefaultIfEmpty()
            //                  group new { joinxa, joinxb, joinxc, joinxd, joinxe } by new { x.photoId, x.regionId, x.categoryId, joinxb?.ImageUrl, joinxc?.RegionName, joinxd?.CategoryName, joinxe?.location, joinxe?.photodesc } into g
            //                  select new
            //                  {
            //                      ImageUrl = g.Key.ImageUrl,
            //                      HashPhotoId = g.Key.photoId,
            //                      RegionId = g.Key.regionId,
            //                      CategoryId = g.Key.categoryId,
            //                      RegionName = g.Key.RegionName,
            //                      CategoryName = g.Key.CategoryName,
            //                      Location = g.Key.location ?? "Not Available",
            //                      Description = g.Key.photodesc ?? "Not Available",
            //                      AverageScore = g.Average(x => x.joinxa != null && x.joinxa.Score != default ? x.joinxa.Score : 0.00m)
            //                  }).ToList().OrderByDescending(x => x.AverageScore);


            //get rank
            List<OverallPhotoRankView> overallRank = new List<OverallPhotoRankView>();
            int rank = 0;
            int rank_ = 0;
            bool unrank = false;
            decimal prevScore = 10.01m;
            foreach (var item in getAverage)
            {
                if (item.AverageScore == 0.00m)
                {
                    unrank = true;
                    rank_ = 0;
                }
                else
                {
                    if (prevScore > item.AverageScore)
                    {
                        rank = rank + 1;
                        rank_ = rank;

                    }
                    else
                    {
                        rank_ = rank;
                    }

                }

                OverallPhotoRankView overallPhotoRankView = new OverallPhotoRankView
                {
                    ImageUrl = item.ImageUrl,
                    HashPhotoId = item.HashPhotoId,
                    AverageScore = item.AverageScore,
                    RegionId = item.RegionId,
                    RegionName = item.RegionName,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    Location = item.Location ?? "Not Available",
                    Description = item.Description ?? "Not Available",
                    PhotoTitle = item.PhotoTitle ?? "Not Available",
                    PhotoTaken = item.PhotoTaken ?? "Not Available",
                    Unranked = unrank,
                    Rank = rank_

                };

                overallRank.Add(overallPhotoRankView);


                prevScore = item.AverageScore;
            }

            return overallRank;
        }

        public List<OverallPhotoRankView> GetGrandFinalRanking(string date, int dayNumber, string round, int categoryId)
        {

            //tempTable ----driver table
            var TempA = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                         from joinab in ab.DefaultIfEmpty()
                         where joinab.CategoryId == categoryId && joinab.PhotoCode == "20"
                         && a.Date == date && a.DayNumber == dayNumber
                         select new DriverRankTable
                         {
                             photoId = joinab.HashPhotoId,
                             regionId = joinab.RegionId,
                             categoryId = joinab.CategoryId,
                             round = round,

                         }).ToList();


            var TemB = (
                            from a in _tallyProgramContext.ImageScoreGrandFinal
                            group new { a } by new { a.PhotoId, a.UserId, a.Round} into b
                            select new
                            { 
                                PhotoId = b.Key.PhotoId,
                                UserId = b.Key.UserId,
                                Round = b.Key.Round,
                                ScoreSum = b.Sum(a => a.a.Score != default ? a.a.Score : 0.00m)
                            }
                        
                        ).ToList();
          



            var getAverage = (from x in TempA
                              join a in TemB
                                    on new { x1 = x.photoId, x2 = x.round } equals new { x1 = a.PhotoId, x2 = a.Round } into xa
                              from joinxa in xa.DefaultIfEmpty()
                              join b in _tallyProgramContext.PhotoMetaData on x.photoId equals b.HashPhotoId into xb
                              from joinxb in xb.DefaultIfEmpty()
                              join c in _tallyProgramContext.TblRegions on x.regionId equals c.RegionId into xc
                              from joinxc in xc.DefaultIfEmpty()
                              join d in _tallyProgramContext.Categories on x.categoryId equals d.CategoryId into xd
                              from joinxd in xd.DefaultIfEmpty()
                              join e in _tallyProgramContext.CSV_PhotoDetails on joinxb.FileName equals e.photoname into xe
                              from joinxe in xe.DefaultIfEmpty()
                              group new { joinxa, joinxb, joinxc, joinxd, joinxe } by new { x.photoId, x.regionId, x.categoryId, joinxb?.ImageUrl, joinxc?.RegionName, joinxd?.CategoryName, joinxe?.location, joinxe?.photodesc, joinxe?.phototitle, joinxe?.datetaken } into g
                              select new
                              {
                                  ImageUrl = g.Key.ImageUrl,
                                  HashPhotoId = g.Key.photoId,
                                  RegionId = g.Key.regionId,
                                  CategoryId = g.Key.categoryId,
                                  RegionName = g.Key.RegionName,
                                  CategoryName = g.Key.CategoryName,
                                  Location = g.Key.location ?? "Not Available",
                                  Description = g.Key.photodesc ?? "Not Available",
                                  PhotoTitle = g.Key.phototitle ?? "Not Available",
                                  PhotoTaken = g.Key.datetaken ?? "Not Available",
                                  AverageScore = g.Average(x => x.joinxa != null && x.joinxa.ScoreSum != default ? x.joinxa.ScoreSum : 0.00m)
                              }).ToList().OrderByDescending(x => x.AverageScore);



            ////get the average and other information
            //var getAverage = (from a in _tallyProgramContext.PhotoLocations
            //                  join b in _tallyProgramContext.ImageBatches on a.HashPhotoId equals b.ImageHashId into ab
            //                  from b in ab.DefaultIfEmpty()
            //                  join c in _tallyProgramContext.ImageScoreGrandFinal on a.HashPhotoId equals c.PhotoId into ac
            //                  from c in ac.DefaultIfEmpty()
            //                  join d in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals d.HashPhotoId into ad
            //                  from d in ad.DefaultIfEmpty()
            //                  join e in _tallyProgramContext.TblRegions on a.RegionId equals e.RegionId into ae
            //                  from e in ae.DefaultIfEmpty()
            //                  join f in _tallyProgramContext.Categories on a.CategoryId equals f.CategoryId into af
            //                  from f in af.DefaultIfEmpty()
            //                  join g in _tallyProgramContext.CSV_PhotoDetails on a.HashPhotoId equals g.photohash into ag
            //                  from g in ag.DefaultIfEmpty()
            //                  where a.CategoryId == categoryId && a.PhotoCode == "20"
            //                  && c.Round == round && b.Date == date && b.DayNumber == dayNumber
            //                  group new { a, b, c, d, e, f, g } by new { a.HashPhotoId, a.RegionId, a.CategoryId, d.ImageUrl, e.RegionName, f.CategoryName, g.location, g.photodesc } into g
            //                  select new
            //                  {
            //                      ImageUrl = g.Key.ImageUrl,
            //                      HashPhotoId = g.Key.HashPhotoId,
            //                      RegionId = g.Key.RegionId,
            //                      CategoryId = g.Key.CategoryId,
            //                      RegionName = g.Key.RegionName,
            //                      CategoryName = g.Key.CategoryName,
            //                      Location = g.Key.location != null ? g.Key.location : "Not Available",
            //                      Description = g.Key.photodesc != null ? g.Key.photodesc : "Not Available",
            //                      AverageScore = g.Average(x => x.c.Score == null ? 0.00m : x.c.Score)
            //                  }).ToList().OrderByDescending(x => x.AverageScore == null ? 0.00m : x.AverageScore);


            //get rank
            List<OverallPhotoRankView> overallRank = new List<OverallPhotoRankView>();
            int rank = 0;
            int rank_ = 0;
            bool unrank = false;
            decimal prevScore = 10.01m;
            foreach (var item in getAverage)
            {
                if (item.AverageScore == 0.00m)
                {
                    unrank = true;
                    rank_ = 0;
                }
                else
                {
                    if (prevScore > item.AverageScore)
                    {
                        rank = rank + 1;
                        rank_ = rank;

                    }
                    else
                    {
                        rank_ = rank;
                    }

                }

                OverallPhotoRankView overallPhotoRankView = new OverallPhotoRankView
                {
                    ImageUrl = item.ImageUrl,
                    HashPhotoId = item.HashPhotoId,
                    AverageScore = item.AverageScore,
                    RegionId = item.RegionId,
                    RegionName = item.RegionName,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    Location = item.Location,
                    Description = item.Description,
                    PhotoTitle = item.PhotoTitle,
                    PhotoTaken = item.PhotoTaken,
                    Unranked = unrank,
                    Rank = rank_

                };

                overallRank.Add(overallPhotoRankView);


                prevScore = item.AverageScore;
            }

            return overallRank;
        }

        public MyRankScore GetImageInfoByHashId(string hashPhotoId, string userId, string round)
        {

            var basicInfo = (from a in _tallyProgramContext.PhotoLocations
                             join b in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals b.HashPhotoId into joinab
                             from ab in joinab.DefaultIfEmpty()
                             join c in _tallyProgramContext.CSV_PhotoDetails on a.HashPhotoId equals c.photohash into joinac
                             from ac in joinac.DefaultIfEmpty()
                             join d in _tallyProgramContext.TblRegions on a.RegionId equals d.RegionId into joinad
                             from ad in joinad.DefaultIfEmpty()
                             join e in _tallyProgramContext.Categories on a.CategoryId equals e.CategoryId into joinae
                             from ae in joinae.DefaultIfEmpty()
                             where a.HashPhotoId == hashPhotoId
                             select new MyRankScore {
                                 ImageUrl = ab.ImageUrl,
                                 ImageSort = 0,
                                 HashPhotoId = a.HashPhotoId,
                                 MyScore = 0.00m,
                                 RegionId = a.RegionId,
                                 RegionName = ad.RegionName,
                                 CategoryId = a.CategoryId,
                                 CategoryName = ae.CategoryName,
                                 Location = ac.location == null ? "Not Available" : ac.location,
                                 Description = ac.photodesc == null ? "Not Available" : ac.photodesc,
                                 PhotoTitle = ac.phototitle == null ? "Not Available" : ac.phototitle,
                                 PhotoTaken = ac.datetaken == null ? "Not Available" : ac.datetaken,
                                 Round = round,
                                 IsUnranked = false,
                             }).ToList();

            var IsExistScoreInfo = (from a in _tallyProgramContext.ImageScore
                             where a.PhotoId == hashPhotoId && a.UserId == userId && a.Round == round
                             select a).Any();


            if (IsExistScoreInfo)
            {
                var joinRecord = (from a in basicInfo
                                  join b in _tallyProgramContext.ImageScore on a.HashPhotoId equals b.PhotoId into joinab
                                  from ab in joinab.DefaultIfEmpty()
                                  where ab.PhotoId == hashPhotoId && ab.UserId == userId && ab.Round == round
                                  select new MyRankScore
                                  {
                                      ImageUrl = a.ImageUrl,
                                      ImageSort = 0,
                                      HashPhotoId = a.HashPhotoId,
                                      MyScore = ab.Score == null ? 0.00m : ab.Score,
                                      RegionId = a.RegionId,
                                      RegionName = a.RegionName,
                                      CategoryId = a.CategoryId,
                                      CategoryName = a.CategoryName,
                                      Location = a.Location,
                                      Description = a.Description,
                                      PhotoTitle = a.PhotoTitle,
                                      PhotoTaken = a.PhotoTaken,
                                      Round = round,
                                      IsUnranked = ab.Score == null ? true : false,
                                  }).ToList();

                return joinRecord.FirstOrDefault();

            }

            return basicInfo.FirstOrDefault();
            
        }
    }
}
