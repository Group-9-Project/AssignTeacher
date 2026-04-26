namespace AssignTeacher.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEntranceTestTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EntranceTests",
                c => new
                    {
                        TestId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(),
                        DurationMinutes = c.Int(nullable: false),
                        QuetionOne = c.String(),
                        AnswerOneAA = c.String(),
                        AnswerOneBB = c.String(),
                        AnswerOneCC = c.String(),
                        CorrectAnswer1 = c.String(),
                        QuetionTwo = c.String(),
                        AnswerTwoAA = c.String(),
                        AnswerTwoBB = c.String(),
                        AnswerTwoCC = c.String(),
                        CorrectAnswer2 = c.String(),
                        QuetionThree = c.String(),
                        AnswerThreeAA = c.String(),
                        AnswerThreeBB = c.String(),
                        AnswerThreeCC = c.String(),
                        CorrectAnswer3 = c.String(),
                        QuetionFour = c.String(),
                        AnswerFourAA = c.String(),
                        AnswerFourBB = c.String(),
                        AnswerFourCC = c.String(),
                        CorrectAnswer4 = c.String(),
                        QuetionFive = c.String(),
                        AnswerFiveAA = c.String(),
                        AnswerFiveBB = c.String(),
                        AnswerFiveCC = c.String(),
                        CorrectAnswer5 = c.String(),
                        a = c.String(),
                        b = c.String(),
                        c = c.String(),
                        counter = c.Int(nullable: false),
                        TotalScore = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.TestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EntranceTests");
        }
    }
}
