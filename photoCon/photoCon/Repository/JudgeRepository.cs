using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;
using photoCon.Helper;
using photoCon.Models;
using photoCon.Services;
using System;

namespace photoCon.Repository
{
    public class JudgeRepository : IJudgeRepository
    {
        private readonly TallyProgramContext _db;
        LocalEncryption _encryptionService = new LocalEncryption();

        public JudgeRepository(TallyProgramContext tallyProgramContext)
        {
            _db = tallyProgramContext;

        }

        public bool CreateJudge(Judge judge)
        {
            _db.Judges.Add(judge);
            return Save();
        }

        public async Task<ICollection<Judge>> GetAllJudge()
        {
            // Use Select to project the result and handle potential null values
            return await _db.Judges.Select(judge => new Judge
            {
                // Provide default values for properties that could be null
                Index = judge.Index,
                FirstName = judge.FirstName ?? "", // Provide an empty string as default value
                LastName = judge.LastName ?? "",
                MiddleName = judge.MiddleName ?? "",
                EmailAddress = judge.EmailAddress ?? "",
                MobileNumber = judge.MobileNumber ?? "",
                Description = judge.Description ?? "",



                // Assign other properties similarly
            }).ToListAsync();
        }

        public Task<Judge> GetJudgeById(int index)
        {
            var judge = (from a in _db.Judges
                              where a.Index == index
                              select a).FirstOrDefaultAsync();

            return judge;
        }

        public bool IsJudgeExist(string lastname, string firstname, string? middlename)
        {
            var isJudgeExist = (from a in _db.Judges
                         where a.FirstName == firstname
                         && a.LastName == lastname  
                         && a.MiddleName == middlename
                         select a).Any();

            return isJudgeExist;
        }

        public IQueryable<ViewPhotoInfo> GetViewPhotoInfosAsync()
        {
            var query = from a in _db.PhotoLocations
                        join b in _db.PhotoMetaData on a.HashPhotoId equals b.HashPhotoId into viewPhotoInfo
                        from joinTbl in viewPhotoInfo.DefaultIfEmpty()
                        select new
                        {
                            ViewPhotoInfo = new ViewPhotoInfo
                            {
                                HashPhotoId = a.HashPhotoId,
                                PhotoLocation = a.RepositoryLocation,
                                RegionId = a.RegionId,
                                Filename = joinTbl.FileName,
                                Dimension = joinTbl.Dimension,
                                Width = joinTbl.Width,
                                Height = joinTbl.Height,
                                HorizontalResolution = joinTbl.HorizontalResolution,
                                VerticalResolution = joinTbl.VerticalResolution,
                                BitDepth = joinTbl.BitDepth,
                                ResolutionUnit = joinTbl.ResolutionUnit,
                                ImageURL = joinTbl.ImageUrl,
                                CameraMaker = joinTbl.CameraMaker,
                                CameraModel = joinTbl.CameraModel,
                                FStop = joinTbl.Fstop,
                                ExposureTime = joinTbl.ExposureTime,
                                ISOSpeed = joinTbl.Isospeed,
                                FocalLength = joinTbl.FocalLength,
                                MaxAperture = joinTbl.MaxAperture,
                                MeteringMode = joinTbl.MeteringMode,
                                FlashMode = joinTbl.FlashMode,
                                MmFocalLength = joinTbl.MmFocalLength,
                            },
                            RowNumber = 0 // This will be replaced with the actual row number
                        };

            // Calculate row number for each item in the query result
            var queryWithRowNumber = query.AsEnumerable().Select((item, index) => new ViewPhotoInfo
            {
                RowNumber = index + 1,
                phoIdx_ = _encryptionService.ANEncrypt((index + 1).ToString(), "thequickbrownfox"),
                HashPhotoId = item.ViewPhotoInfo.HashPhotoId,
                PhotoLocation = item.ViewPhotoInfo.PhotoLocation,
                RegionId = item.ViewPhotoInfo.RegionId,
                Filename = item.ViewPhotoInfo.Filename,
                Dimension = item.ViewPhotoInfo.Dimension,
                Width = item.ViewPhotoInfo.Width,
                Height = item.ViewPhotoInfo.Height,
                HorizontalResolution = item.ViewPhotoInfo.HorizontalResolution,
                VerticalResolution = item.ViewPhotoInfo.VerticalResolution,
                BitDepth = item.ViewPhotoInfo.BitDepth,
                ResolutionUnit = item.ViewPhotoInfo.ResolutionUnit,
                ImageURL = item.ViewPhotoInfo.ImageURL,
                CameraMaker = item.ViewPhotoInfo.CameraMaker,
                CameraModel = item.ViewPhotoInfo.CameraModel,
                FStop = item.ViewPhotoInfo.FStop,
                ExposureTime = item.ViewPhotoInfo.ExposureTime,
                ISOSpeed = item.ViewPhotoInfo.ISOSpeed,
                FocalLength = item.ViewPhotoInfo.FocalLength,
                MaxAperture = item.ViewPhotoInfo.MaxAperture,
                MeteringMode = item.ViewPhotoInfo.MeteringMode,
                FlashMode = item.ViewPhotoInfo.FlashMode,
                MmFocalLength = item.ViewPhotoInfo.MmFocalLength,
            });

            return queryWithRowNumber.AsQueryable();
        }

        //public async Task<IQueryable<JudgeImageView>> GetJudgeImageViewAsync()
        //{
        //    return _db.JudgeImageView.AsQueryable();
        //}


        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        //public IQueryable<JudgeView> GetAllJudgeView()
        //{
        //    var query = from j in _db.Judges
        //                select new JudgeView
        //                {
        //                    Index = j.Index,
        //                    HashIndex = j.HashIndex,
        //                    FullName = j.FirstName + " " + j.MiddleName + " " + j.LastName,
        //                    EmailAddress = j.EmailAddress,
        //                    MobileNumber = j.MobileNumber,
        //                    Description = j.Description
        //                };

        //    var result = query.OrderBy(a => a.Index);

        //    return result;
        //}

        IQueryable<JudgeImageView> IJudgeRepository.GetJudgeImageViewAsync()
        {
            return _db.JudgeImageView.AsQueryable();
        }

        public decimal GetImageScore(string photoId, string userId)
        {
            var imageScore = (from a in _db.ImageScore
                             where a.PhotoId == photoId
                             && a.UserId == userId
                             select a.Score).FirstOrDefault();

            return imageScore;
        }

        public List<ImageScore> GetAllJudgeScore(string userId, string refTypeId, string refCodeId)
        {
           var round   = refTypeId.ToString() + refCodeId.ToString();
           var scoreList = (from a in _db.ImageScore
                            where a.UserId == userId
                            && a.Round == round
                            select a).ToList();

            return scoreList;
        }

        public bool IsImageScoreByUser(string userid, string photoid, string refTypeId, string refCodeId)
        {
            var round = refTypeId.ToString() + refCodeId.ToString();
            var istrue = (from a in _db.ImageScore
                          where a.PhotoId == photoid
                          && a.UserId == userid
                          && a.Round == round
                          select a).Any();

            return istrue;
        }

        public bool IsImageScoreGrandFinalByUser(string userid, string photoid, string refTypeId, string refCodeId)
        {
            var round = refTypeId.ToString() + refCodeId.ToString();
            var istrue = (from a in _db.ImageScoreGrandFinal
                          where a.PhotoId == photoid
                          && a.UserId == userid
                          && a.Round == round
                          select a).Any();

            return istrue;
        }

        public bool SaveScoreByJudgeInImage(ImageScore imageScore)
        {
            _db.ImageScore.Add(imageScore);
            return Save();
        }

        public bool SaveScoreByJudgeInImagePerCriteria(ImageScoreGrandFinal imageScore)
        {
            _db.ImageScoreGrandFinal.Add(imageScore);
            return Save();
        }

        public bool UpdateScoreByJudgeInImage(string userid, string photoid, decimal score, string refTypeId, string refCodeId,  DateTime lastupdatedate)
        {
            var round = refTypeId.ToString() + refCodeId.ToString();
            ImageScore imageScore = (from a in _db.ImageScore
                                     where a.UserId == userid
                                     && a.PhotoId == photoid
                                     && a.Round == round
                                     select a).SingleOrDefault();

            imageScore.Score = score;
            imageScore.LastUpdateDate = lastupdatedate;

            return Save();
        }

        public List<ImageScore> GetImageScoreByJudgeAndRound(string userId, string refTypeId, string refCodeId, string photoId)
        {

            var round = refTypeId.ToString() + refCodeId.ToString();
            var scoreList = (from a in _db.ImageScore
                             where a.UserId == userId
                             && a.Round == round
                             && a.PhotoId == photoId
                             select a).ToList();

            return scoreList;

        }



        public List<ImageScoreGrandFinal> GetImageScoreGrandFinalByJudgeAndRound(string userId, string refTypeId, string refCodeId, string photoId)
        {

            var round = refTypeId.ToString() + refCodeId.ToString();
            var scoreList = (from a in _db.ImageScoreGrandFinal
                             where a.UserId == userId
                             && a.Round == round
                             && a.PhotoId == photoId
                             select a).ToList();

            return scoreList;

        }

        public bool UpdateScoreByJudgeInImagePerCriteria(ImageScoreGrandFinal imageScore)
        {
            var existingRecord = _db.ImageScoreGrandFinal
            .FirstOrDefault(x => x.UserId == imageScore.UserId &&
                                      x.PhotoId == imageScore.PhotoId &&
                                      x.Round == imageScore.Round &&
                                      x.Criteria == imageScore.Criteria);

            if (existingRecord != null)
            {
                existingRecord.Score = imageScore.Score;
                existingRecord.LastUpdateDate = DateTime.Now;
            }

            return Save();
        }





       

    }
}
