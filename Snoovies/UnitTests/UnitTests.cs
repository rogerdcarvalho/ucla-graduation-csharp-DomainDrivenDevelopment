using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snoovies.Domain;
using Snoovies.DataAccess;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        // Arrange
        public Film testFilm = null;
        public string title = "Test Film";
        public string description = "This is a test film created by Visual Studio";
        public string longDescription = "This film was created by Visual Studio as part of the unit test method CreateFilm()";
        public string videoLink = "https://player.vimeo.com/external/116485479.m3u8?p=high,standard,mobile&s=ecd12952aeed14b3e2ee6df69038c485";
        public string imageLink = "http://snoovies.com/blog/wp-content/uploads/2014/12/cropped-logo1.png";
        public TimeSpan videoLength = new TimeSpan(0, 0, 2, 46);
        public DateTime dateCreated = new DateTime(2015, 10, 23);
        public int testFilmId;
        public Repository<Film> testRepository;

        [TestMethod]
        public void CreateUpdateAndDeleteFilm()
        {
       
            //Enter film data
            testFilm = new Film();
            testFilm.Title = title;
            testFilm.Description = description;
            testFilm.LongDescription = longDescription;
            testFilm.VideoLink = videoLink;
            testFilm.ImageLink = imageLink;
            testFilm.TimeStamp = dateCreated;
            testFilm.Time = videoLength;

            //Sync film data with database and reload film data from database
            testRepository = new Repository<Film>();
            testRepository.Persist(testFilm);

            //Get the testFilmId
            testFilmId = testFilm.Id;

            //ReImport the test film from the database
            Criteria testFilmCriteria = new Criteria();
            testFilmCriteria.FieldName = "id";
            testFilmCriteria.Expression = testFilmId;
            IEnumerable<Film> importedData = testRepository.Fetch(testFilmCriteria);
            Film reimportedTestFilm = importedData.Single();

            // Assert
            Assert.IsInstanceOfType(testFilm, typeof(Film));
            Assert.AreEqual(title, testFilm.Title);
            Assert.AreEqual(description, testFilm.Description);
            Assert.AreEqual(longDescription, testFilm.LongDescription);
            Assert.AreEqual(videoLink, testFilm.VideoLink);
            Assert.AreEqual(imageLink, testFilm.ImageLink);
            Assert.AreEqual(dateCreated, testFilm.TimeStamp);
            Assert.AreEqual(videoLength, testFilm.Time);

            Assert.IsInstanceOfType(reimportedTestFilm, typeof(Film));
            Assert.AreEqual(title, reimportedTestFilm.Title);
            Assert.AreEqual(description, reimportedTestFilm.Description);
            Assert.AreEqual(longDescription, reimportedTestFilm.LongDescription);
            Assert.AreEqual(videoLink, reimportedTestFilm.VideoLink);
            Assert.AreEqual(imageLink, reimportedTestFilm.ImageLink);
            Assert.AreEqual(dateCreated, reimportedTestFilm.TimeStamp);
            Assert.AreEqual(videoLength, reimportedTestFilm.Time);

            //Update the film
            string updatedTitle = "Updated Test Film";
            testFilm.Title = updatedTitle;
            testRepository.Persist(testFilm);

            //ReImport the test film from the database
            testFilmCriteria = new Criteria();
            testFilmCriteria.FieldName = "id";
            testFilmCriteria.Expression = testFilmId;
            importedData = testRepository.Fetch(testFilmCriteria);
            reimportedTestFilm = importedData.Single();

            Assert.AreEqual(updatedTitle, testFilm.Title);
            Assert.AreEqual(updatedTitle, reimportedTestFilm.Title);

            //Delete the film created
            testFilm.IsMarkedForDeletion = true;
            testFilm = testRepository.Persist(testFilm);
            importedData = testRepository.Fetch(testFilmCriteria);
            reimportedTestFilm = importedData.SingleOrDefault();

            //Assert
            Assert.IsNull(testFilm);
            Assert.IsNull(reimportedTestFilm);

        }

        [TestMethod]
        public void CreateUpdateAndDeleteFilmWithBTS()
        {

            //Enter film data
            testFilm = new Film();
            testFilm.Title = title;
            testFilm.Description = description;
            testFilm.LongDescription = longDescription;
            testFilm.VideoLink = videoLink;
            testFilm.ImageLink = imageLink;
            testFilm.TimeStamp = dateCreated;
            testFilm.Time = videoLength;

            //Enter BTS Data
            string introtext =  "Test intro text created by Visual Studio. This intro text was created by the CreateFilmWithBTS() unit test";
            string headline = "Test Headline";
            string body =  "This is a test behind-the-scenes section created by Visual Studio";
            BehindTheScenesSection section1 = new BehindTheScenesSection();
            BehindTheScenesSection section2 = new BehindTheScenesSection();
            BehindTheScenesSection section3 = new BehindTheScenesSection();
            section1.IntroText = introtext;
            section1.Position = 0;
            section2.Headline = headline;
            section2.Position = 1;
            section2.Body = body;
            section3.Headline = headline;
            section3.Position = 1;
            section3.Body = body;

            //Link BTS sections with film
            //testFilm.LinkedDomains.Add(section1);
            //testFilm.LinkedDomains.Add(section2);
            //testFilm.LinkedDomains.Add(section3);
            section1.Parent = testFilm;
            section2.Parent = testFilm;
            section3.Parent = testFilm;

            //Sync film data with database and reload film data from database
            testRepository = new Repository<Film>();
            testRepository.Persist(testFilm);
    
            //Get the ids
            testFilmId = testFilm.Id;
            int section1Id = section1.Id;
            int section2Id = section2.Id;
            int section3Id = section3.Id;

            //ReImport the test film from the database
            Criteria testFilmCriteria = new Criteria();
            testFilmCriteria.FieldName = "id";
            testFilmCriteria.Expression = testFilmId;
            IEnumerable<Film> importedData = testRepository.Fetch(testFilmCriteria);
            Film reimportedTestFilm = importedData.Single();


            // Assert
            Assert.IsInstanceOfType(testFilm, typeof(Film));
            Assert.AreEqual(title, testFilm.Title);
            Assert.AreEqual(description, testFilm.Description);
            Assert.AreEqual(longDescription, testFilm.LongDescription);
            Assert.AreEqual(videoLink, testFilm.VideoLink);
            Assert.AreEqual(imageLink, testFilm.ImageLink);
            Assert.AreEqual(dateCreated, testFilm.TimeStamp);
            Assert.AreEqual(videoLength, testFilm.Time);

            Assert.IsTrue(testFilm.BehindTheScenes.Count() == 3);
            Assert.AreEqual(introtext, testFilm.BehindTheScenes.Where(o => o.Id == section1Id).SingleOrDefault().IntroText);
            Assert.AreEqual(headline, testFilm.BehindTheScenes.Where(o => o.Id == section2Id).SingleOrDefault().Headline);
            Assert.AreEqual(body, testFilm.BehindTheScenes.Where(o => o.Id == section2Id).SingleOrDefault().Body);

            Assert.IsInstanceOfType(reimportedTestFilm, typeof(Film));
            Assert.AreEqual(title, reimportedTestFilm.Title);
            Assert.AreEqual(description, reimportedTestFilm.Description);
            Assert.AreEqual(longDescription, reimportedTestFilm.LongDescription);
            Assert.AreEqual(videoLink, reimportedTestFilm.VideoLink);
            Assert.AreEqual(imageLink, reimportedTestFilm.ImageLink);
            Assert.AreEqual(dateCreated, reimportedTestFilm.TimeStamp);
            Assert.AreEqual(videoLength, reimportedTestFilm.Time);

            Assert.IsTrue(reimportedTestFilm.BehindTheScenes.Count() == 3);
            Assert.AreEqual(introtext, reimportedTestFilm.BehindTheScenes.Where(o => o.Id == section1Id).SingleOrDefault().IntroText);
            Assert.AreEqual(headline, reimportedTestFilm.BehindTheScenes.Where(o => o.Id == section2Id).SingleOrDefault().Headline);
            Assert.AreEqual(body, reimportedTestFilm.BehindTheScenes.Where(o => o.Id == section2Id).SingleOrDefault().Body);

            //Update one of the BTS Sections
            Repository<BehindTheScenesSection> testBtsRepository = new Repository<BehindTheScenesSection>();
            IEnumerable<BehindTheScenesSection> importedBTS;
            string updatedHeadline = "Updated Headline";
            section2.Headline = updatedHeadline;
            section2 = testBtsRepository.Persist(section2);

            Criteria testSectionCriteria = new Criteria();
            testSectionCriteria.FieldName = "id";
            testSectionCriteria.Expression = section2Id;
            importedBTS = testBtsRepository.Fetch(testSectionCriteria);
            BehindTheScenesSection reimportedSection2 = importedBTS.SingleOrDefault();
            
            Assert.AreEqual(updatedHeadline, section2.Headline);
            Assert.AreEqual(updatedHeadline, reimportedSection2.Headline);
 
            //Delete one of the BTS sections individually
            section3.IsMarkedForDeletion = true;
            section3 = testBtsRepository.Persist(section3);

            testSectionCriteria = new Criteria();
            testSectionCriteria.FieldName = "id";
            testSectionCriteria.Expression = section3Id;
            importedBTS = testBtsRepository.Fetch(testSectionCriteria);
            BehindTheScenesSection reimportedSection3 = importedBTS.SingleOrDefault();

            //Delete the film created and all remaining BTS sections
            testFilm.IsMarkedForDeletion = true;
            testFilm = testRepository.Persist(testFilm);
            importedData = testRepository.Fetch(testFilmCriteria);
            reimportedTestFilm = importedData.SingleOrDefault();

            testSectionCriteria = new Criteria();
            testSectionCriteria.FieldName = "id";
            testSectionCriteria.Expression = section1Id;
            importedBTS = testBtsRepository.Fetch(testSectionCriteria);
            BehindTheScenesSection reimportedSection1 = importedBTS.SingleOrDefault();

            testSectionCriteria = new Criteria();
            testSectionCriteria.FieldName = "id";
            testSectionCriteria.Expression = section2Id;
            importedBTS = testBtsRepository.Fetch(testSectionCriteria);
            reimportedSection2 = importedBTS.SingleOrDefault();

            //Assert
            Assert.IsNull(testFilm);
            Assert.IsNull(reimportedTestFilm);
            Assert.IsNull(reimportedSection1);
            Assert.IsNull(reimportedSection2);
            Assert.IsNull(reimportedSection3);
        }
        [TestMethod]
        public void CreateUpdateAndDeleteFilmWithCredits()
        {

            //Enter film data
            testFilm = new Film();
            testFilm.Title = title;
            testFilm.Description = description;
            testFilm.LongDescription = longDescription;
            testFilm.VideoLink = videoLink;
            testFilm.ImageLink = imageLink;
            testFilm.TimeStamp = dateCreated;
            testFilm.Time = videoLength;

            //Enter Credit Data
            string persons = "Test Person";
            string role = "Test Role";
            Credit credit1 = new Credit();
            Credit credit2 = new Credit();
            Credit credit3 = new Credit();
            credit1.Role = role + " 1";
            credit1.Persons = persons + " 1";
            credit1.Primary = true;
            credit2.Role = role + " 2";
            credit2.Persons = persons + " 2";
            credit2.Primary = false;
            credit3.Role = role + " 3";
            credit3.Persons = persons + " 3";
            credit3.Primary = false;


            //Link credits with film
            //testFilm.LinkedDomains.Add(credit1);
            //testFilm.LinkedDomains.Add(credit2);
            //testFilm.LinkedDomains.Add(credit3);
            credit1.Parent = testFilm;
            credit2.Parent = testFilm;
            credit3.Parent = testFilm;

            //Sync film data with database and reload film data from database
            testRepository = new Repository<Film>();
            testRepository.Persist(testFilm);

            //Get the ids
            testFilmId = testFilm.Id;
            int credit1Id = credit1.Id;
            int credit2Id = credit2.Id;
            int credit3Id = credit3.Id;

            //ReImport the test film from the database
            Criteria testFilmCriteria = new Criteria();
            testFilmCriteria.FieldName = "id";
            testFilmCriteria.Expression = testFilmId;
            IEnumerable<Film> importedData = testRepository.Fetch(testFilmCriteria);
            Film reimportedTestFilm = importedData.Single();


            // Assert
            Assert.IsInstanceOfType(testFilm, typeof(Film));
            Assert.AreEqual(title, testFilm.Title);
            Assert.AreEqual(description, testFilm.Description);
            Assert.AreEqual(longDescription, testFilm.LongDescription);
            Assert.AreEqual(videoLink, testFilm.VideoLink);
            Assert.AreEqual(imageLink, testFilm.ImageLink);
            Assert.AreEqual(dateCreated, testFilm.TimeStamp);
            Assert.AreEqual(videoLength, testFilm.Time);

            Assert.IsTrue(testFilm.Credits.Count() == 3);
            Assert.AreEqual(persons + " 1", testFilm.Credits.Where(o => o.Id == credit1Id).SingleOrDefault().Persons);
            Assert.AreEqual(role + " 1", testFilm.Credits.Where(o => o.Id == credit1Id).SingleOrDefault().Role);
            Assert.IsTrue(testFilm.Credits.Where(o => o.Id == credit1Id).SingleOrDefault().Primary);
            Assert.AreEqual(persons + " 2", testFilm.Credits.Where(o => o.Id == credit2Id).SingleOrDefault().Persons);
            Assert.AreEqual(role + " 2", testFilm.Credits.Where(o => o.Id == credit2Id).SingleOrDefault().Role);
            Assert.IsFalse(testFilm.Credits.Where(o => o.Id == credit2Id).SingleOrDefault().Primary);
        
            Assert.IsInstanceOfType(reimportedTestFilm, typeof(Film));
            Assert.AreEqual(title, reimportedTestFilm.Title);
            Assert.AreEqual(description, reimportedTestFilm.Description);
            Assert.AreEqual(longDescription, reimportedTestFilm.LongDescription);
            Assert.AreEqual(videoLink, reimportedTestFilm.VideoLink);
            Assert.AreEqual(imageLink, reimportedTestFilm.ImageLink);
            Assert.AreEqual(dateCreated, reimportedTestFilm.TimeStamp);
            Assert.AreEqual(videoLength, reimportedTestFilm.Time);

            Assert.IsTrue(reimportedTestFilm.Credits.Count() == 3);
            Assert.AreEqual(persons + " 1", reimportedTestFilm.Credits.Where(o => o.Id == credit1Id).SingleOrDefault().Persons);
            Assert.AreEqual(role + " 1", reimportedTestFilm.Credits.Where(o => o.Id == credit1Id).SingleOrDefault().Role);
            Assert.AreEqual(persons + " 2", reimportedTestFilm.Credits.Where(o => o.Id == credit2Id).SingleOrDefault().Persons);
            Assert.AreEqual(role + " 2", reimportedTestFilm.Credits.Where(o => o.Id == credit2Id).SingleOrDefault().Role);

            //Update an individual credit
            string updatedPersons = "Updated Person";
            Repository<Credit> testCreditsRepository = new Repository<Credit>();
            IEnumerable<Credit> importedCredits;
            credit3.Persons = updatedPersons;
            credit3 = testCreditsRepository.Persist(credit3);

            Criteria testCreditsCriteria = new Criteria();
            testCreditsCriteria.FieldName = "id";
            testCreditsCriteria.Expression = credit3Id;

            importedCredits = testCreditsRepository.Fetch(testCreditsCriteria);
            Credit reimportedCredit3 = importedCredits.SingleOrDefault();

            Assert.AreEqual(updatedPersons, credit3.Persons);
            Assert.AreEqual(updatedPersons, reimportedCredit3.Persons);

            //Delete an individual credit
            credit3.IsMarkedForDeletion = true;
            testCreditsRepository.Persist(credit3);

            importedCredits = testCreditsRepository.Fetch(testCreditsCriteria);
            reimportedCredit3 = importedCredits.SingleOrDefault();

            //Delete the film created and remaining credits
            testFilm.IsMarkedForDeletion = true;
            testFilm = testRepository.Persist(testFilm);
            importedData = testRepository.Fetch(testFilmCriteria);
            reimportedTestFilm = importedData.SingleOrDefault();

            //Try to request the credit records from the database
            testCreditsCriteria = new Criteria();
            testCreditsCriteria.FieldName = "id";
            testCreditsCriteria.Expression = credit1Id;
            importedCredits = testCreditsRepository.Fetch(testCreditsCriteria);
            Credit reimportedCredit1 = importedCredits.SingleOrDefault();

            testCreditsCriteria.FieldName = "id";
            testCreditsCriteria.Expression = credit2Id;
            importedCredits = testCreditsRepository.Fetch(testCreditsCriteria);
            Credit reimportedCredit2 = importedCredits.SingleOrDefault();

            //Assert
            Assert.IsNull(testFilm);
            Assert.IsNull(reimportedTestFilm);
            Assert.IsNull(reimportedCredit1);
            Assert.IsNull(reimportedCredit2);
        }
        [TestMethod]
        public void CreateUpdateAndDeleteFilmWithReachOut()
        {

            //Enter film data
            testFilm = new Film();
            testFilm.Title = title;
            testFilm.Description = description;
            testFilm.LongDescription = longDescription;
            testFilm.VideoLink = videoLink;
            testFilm.ImageLink = imageLink;
            testFilm.TimeStamp = dateCreated;
            testFilm.Time = videoLength;

            //Enter Credit Data
            string reachOutContent = "Test Reach Out content, created by Visual Studio.";
            ReachOutContent reachOutPage = new ReachOutContent();
            reachOutPage.Content = reachOutContent;


            //Link credits with film
            //testFilm.LinkedDomains.Add(reachOutPage);
            reachOutPage.Parent = testFilm;

            //Sync film data with database and reload film data from database
            testRepository = new Repository<Film>();
            testRepository.Persist(testFilm);

            //Get the ids
            testFilmId = testFilm.Id;
            int reachOutId = reachOutPage.Id;
          
            //ReImport the test film from the database
            Criteria testFilmCriteria = new Criteria();
            testFilmCriteria.FieldName = "id";
            testFilmCriteria.Expression = testFilmId;
            IEnumerable<Film> importedData = testRepository.Fetch(testFilmCriteria);
            Film reimportedTestFilm = importedData.Single();


            // Assert
            Assert.IsInstanceOfType(testFilm, typeof(Film));
            Assert.AreEqual(title, testFilm.Title);
            Assert.AreEqual(description, testFilm.Description);
            Assert.AreEqual(longDescription, testFilm.LongDescription);
            Assert.AreEqual(videoLink, testFilm.VideoLink);
            Assert.AreEqual(imageLink, testFilm.ImageLink);
            Assert.AreEqual(dateCreated, testFilm.TimeStamp);
            Assert.AreEqual(videoLength, testFilm.Time);

            Assert.IsTrue(testFilm.ReachOutContent.Count() == 1);
            Assert.AreEqual(reachOutContent, testFilm.ReachOutContent.Single().Content);
         
            Assert.IsInstanceOfType(reimportedTestFilm, typeof(Film));
            Assert.AreEqual(title, reimportedTestFilm.Title);
            Assert.AreEqual(description, reimportedTestFilm.Description);
            Assert.AreEqual(longDescription, reimportedTestFilm.LongDescription);
            Assert.AreEqual(videoLink, reimportedTestFilm.VideoLink);
            Assert.AreEqual(imageLink, reimportedTestFilm.ImageLink);
            Assert.AreEqual(dateCreated, reimportedTestFilm.TimeStamp);
            Assert.AreEqual(videoLength, reimportedTestFilm.Time);

            Assert.IsTrue(reimportedTestFilm.ReachOutContent.Count() == 1);
            Assert.AreEqual(reachOutContent, reimportedTestFilm.ReachOutContent.Single().Content);

            //Update reach out content
            string updatedReachOutContent = "Updated Reach Out content, created by Visual Studio.";
            reachOutPage.Content = updatedReachOutContent;

            Repository<ReachOutContent> testReachOutRepository = new Repository<ReachOutContent>();
            IEnumerable<ReachOutContent> importedReachOut;
            reachOutPage = testReachOutRepository.Persist(reachOutPage);

            //Try to request the reach out content from the database
            Criteria testReachOutCriteria = new Criteria();
            testReachOutCriteria.FieldName = "id";
            testReachOutCriteria.Expression = reachOutId;

            importedReachOut = testReachOutRepository.Fetch(testReachOutCriteria);
            ReachOutContent reimportedReachOut = importedReachOut.SingleOrDefault();

            Assert.AreEqual(updatedReachOutContent, reachOutPage.Content);
            Assert.AreEqual(updatedReachOutContent, reimportedReachOut.Content);

            //Delete the film created and reach out content
            testFilm.IsMarkedForDeletion = true;
            testFilm = testRepository.Persist(testFilm);
            importedData = testRepository.Fetch(testFilmCriteria);
            reimportedTestFilm = importedData.SingleOrDefault();

            importedReachOut = testReachOutRepository.Fetch(testReachOutCriteria);
            reimportedReachOut = importedReachOut.SingleOrDefault();

            //Assert
            Assert.IsNull(testFilm);
            Assert.IsNull(reimportedTestFilm);
            Assert.IsNull(reimportedReachOut);
        }
    }
}
