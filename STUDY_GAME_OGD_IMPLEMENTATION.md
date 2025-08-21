# Study Game OGD Integration & A/B Testing Implementation

## What Was Implemented

### 1. Enhanced OpenGameData (OGD) Logging
- **StudyGameLogger.cs**: Comprehensive logging system specifically for the study mini-game
- **Choice Logging Available**: OGD choice logging exists in ChoiceNode.cs but remains commented out (can be enabled when needed)
- **Enhanced Logging.cs**: Exposed the OGD logger for other systems to use

### 2. Multiple Question Sets for A/B Testing
- **tkam_minigame_questions.json**: Original "To Kill a Mockingbird" questions (existing game file)
- **expanded_question_sets.json**: New question sets covering:
  - Campus Social Issues (identity, ethics, community)
  - Academic Success Strategies (study skills)

### 3. A/B Testing System
- **EnhancedStudyQuestionLoader.cs**: Automatically assigns players to test groups:
  - **Control Group (50%)**: Original TKAM questions
  - **Experimental Group (50%)**: Mixed campus social issues + academic success questions

### 4. Enhanced Game Manager
- **EnhancedFivePositionsGameManager.cs**: Integrated version with full OGD logging

## Data Being Collected

### Study Session Analytics
- Session start/end times
- Question set used (for A/B comparison)
- Player demographics (early/mid/late game)
- Final scores and completion rates

### Question-Level Analytics
- Individual question attempts
- Response accuracy
- Time to answer
- Question difficulty ratings
- Learning progression

### A/B Testing Data
- Test group assignments
- Engagement scores
- Learning outcomes
- Comparative performance metrics

## How A/B Testing Works

Players are automatically assigned to one of two groups based on their device ID:
1. **Control (50%)**: Original TKAM questions
2. **Experimental (50%)**: Mixed campus social issues and academic success questions

This ensures consistent assignment - the same player always gets the same test group. The experimental group gets variety by randomly selecting between campus social issues and academic success themes each session.

## File Structure
```
Assets/
├── Scripts/
│   ├── StudyGameLogger.cs                    # Main OGD logging for study game
│   ├── Logging.cs                           # Enhanced main logger (public property added)
│   ├── Testing/
│   │   └── StudyGameTester.cs               # Comprehensive testing script
│   └── Mini Games/Study/
│       ├── EnhancedStudyQuestionLoader.cs   # A/B testing question loader
│       └── EnhancedFivePositionsGameManager.cs # Enhanced game manager with OGD
└── Resources/Data/
    ├── tkam_minigame_questions.json         # Original TKAM questions (existing file)
    └── expanded_question_sets.json          # New campus & academic question sets (5-letter answers)
```

## How to Test Manually

### What You'll See in the Study Game

The study game is a **5-letter word puzzle** where:
1. **Question/Definition appears** at the top of the screen
2. **5 empty boxes** represent the letters of the answer
3. **Letters fall from the top** of the screen
4. **Drag correct letters** into the correct positions
5. **Timer counts down** (in Solo mode)
6. **Score increases** for each completed word

### Testing Different Question Sets

#### Method 1: Force Specific Question Sets (Easiest)
1. **In Unity Editor**, navigate to the Study scene
2. **Find the EnhancedStudyQuestionLoader component** in the scene
3. **Set `forceQuestionSetId`** to one of:
   - `"tkam_themes"` - Original TKAM questions (falls back to original file format)
   - `"campus_social_issues"` - Campus life themes
   - `"academic_success"` - Study skills themes
4. **Press Play** to test that specific set

#### Method 2: Test A/B Assignment (Realistic)
1. **Play the game normally** - each device gets assigned automatically
2. **Check Console logs** for: `"Player assigned to A/B test group: [group_name]"`
3. **Different devices/users** will get different question sets

### Sample Questions You'll See

#### TKAM Themes (Control Group):
- **Question**: "See it your way - or mine?"
- **Answer**: ANGLE 
- **Definition**: "A particular way of looking at something"

#### Campus Social Issues (Experimental):
- **Question**: "A distinguishing quality you have"
- **Answer**: TRAIT
- **Definition**: "A characteristic feature of your personality"

#### Academic Success (Experimental):
- **Question**: "Concentrated mental effort"  
- **Answer**: FOCUS
- **Definition**: "The center of interest or activity"

### Verifying OGD Logging

#### In Unity Console:
1. **Open Console window** (Window → General → Console)
2. **Look for log messages** like:
   - `"StudyGameLogger connected to main OGD logger"`
   - `"Player assigned to A/B test group: [group]"`
   - `"Question set info: Set: [set_id], A/B Group: [group]"`

#### In Debug Mode:
1. **Enable `debugMode = true`** in the Logging component
2. **Watch for OGD event logs** in the console
3. **Events logged include**:
   - `study_session_start`
   - `study_question_attempt` 
   - `study_session_end`
   - `ab_test_data`

### Scenarios Where Study Game Appears

The study game appears in these contexts:

1. **Solo Study**: Player chooses to study alone in dorm room
2. **Group Study**: Study sessions with other characters
3. **Exam Mode**: During test/quiz scenarios in the story
4. **Scheduled Study**: Part of the weekly campus activities

### Testing Different Game Modes

Each mode has different behavior:
- **Solo Mode**: Timer-based, random letter spawning
- **Group Mode**: No timer, sequential spawning, character interactions
- **Exam Mode**: No timer, performance affects story outcomes

### Troubleshooting

#### If Study Game Doesn't Start:
1. **Check that question JSON files exist** in `Assets/Resources/Data/`
2. **Verify EnhancedStudyQuestionLoader** has the JSON files assigned
3. **Look for error messages** in Unity Console

#### If Questions Don't Change:
1. **Clear PlayerPrefs** (Edit → Clear All PlayerPrefs in Unity)
2. **Check `forceQuestionSetId`** isn't overriding A/B testing
3. **Verify JSON files** are properly formatted

#### If Game Crashes or Behaves Strangely:
1. **Check answer lengths** in JSON files - all answers must be exactly 5 letters
2. **✅ Fixed**: All answers in `expanded_question_sets.json` are now exactly 5 letters
3. **Use test script**: Run the `TestABStudySystem` component to verify everything works

## Comprehensive Testing

### Automated Testing Script

A comprehensive test script has been created: `Assets/Scripts/TestABStudySystem.cs`

#### How to Use the Test Script:
1. **Add to scene**: Create an empty GameObject and attach the `TestABStudySystem` component
2. **Configure testing**: Set `runTestsOnStart = true` for automatic testing, or use the context menu
3. **Run tests**: Either start the scene or right-click the component and select "Run All Tests"

#### Tests Included:
- ✅ **Question Set Loading**: Verifies JSON files load correctly
- ✅ **A/B Testing Assignment**: Confirms proper group distribution
- ✅ **Answer Length Validation**: Ensures all answers are exactly 5 letters
- ✅ **OGD Logging**: Tests logging functionality
- ✅ **Force Question Sets**: Validates manual question set override

#### Expected Output:
```
=== STARTING COMPREHENSIVE STUDY GAME TESTS ===
--- Testing Question Set Loading ---
✅ EnhancedStudyQuestionLoader found
✅ Questions loaded successfully: 5 questions
--- Testing A/B Testing Assignment ---
✅ A/B Test Distribution (simulated): Control: 4/10, Experimental: 6/10
✅ Current assignment: Set: campus_social_issues, A/B Group: experimental
--- Testing Answer Lengths (Must be exactly 5 letters) ---
✅ All answers in expanded_question_sets.json are exactly 5 letters!
✅ All answers in tkam_minigame_questions.json are exactly 5 letters!
--- Testing OGD Logging ---
✅ StudyGameLogger instance found
✅ OGD logging test completed
--- Testing Force Question Set Feature ---
✅ Successfully loaded 5 questions for tkam_themes
✅ Successfully loaded 5 questions for campus_social_issues  
✅ Successfully loaded 5 questions for academic_success
=== ALL TESTS COMPLETED ===
```

#### If Logging Doesn't Work:
1. **Ensure OGD package** is properly installed (`com.fieldday.opengamedata-unity`)
2. **Check Logging component** has valid `appId` and `appVersion`
3. **Enable `debugMode = true`** to see detailed logs

### Testing Checklist

- [ ] Study game loads and starts
- [ ] Questions display correctly
- [ ] Letters spawn and can be dragged
- [ ] Correct answers register properly
- [ ] Timer works (in Solo mode)
- [ ] Score updates correctly
- [ ] Different question sets load
- [ ] A/B test assignment appears in logs
- [ ] OGD events log to console
- [ ] Game transitions back to story properly

### Building for Distribution Testing

If you want to test with multiple users (for real A/B testing):

1. **File → Build Settings** in Unity
2. **Add Open Scenes** (ensure Main.unity is included)
3. **Select Platform** (PC, Mac, WebGL)
4. **Build** the game
5. **Distribute to test users**
6. **Each user gets different A/B assignment** based on their device

### Data Collection

Once OGD is properly configured, data will be sent to your OpenGameData server. You can analyze:
- **Question completion rates** by theme
- **Time spent per question type**  
- **Learning progression patterns**
- **A/B test effectiveness**

## Usage Instructions

### For Researchers/Data Scientists

1. **Enable Full OGD Logging**:
   - Set `debugMode = true` in Logging component
   - Configure `appId` and `appVersion` appropriately
   - **Uncomment choice logging** in ChoiceNode.cs (line 559):
     ```csharp
     // Change this:
     // Logging.Instance.LogPlayerChoice(Name_Of_Choice, choice_number, originalOrder, randomizedOrder);
     // To this:
     Logging.Instance.LogPlayerChoice(Name_Of_Choice, choice_number, originalOrder, randomizedOrder);
     ```

2. **Monitor A/B Test Distribution**:
   ```csharp
   // Check current player's test group (via debug logs)
   // The currentABTestGroup field is private - monitor via console logs:
   // "Player assigned to A/B test group: [group_name]"
   ```

3. **Force Specific Question Sets** (for testing):
   ```csharp
   // In EnhancedStudyQuestionLoader inspector
   forceQuestionSetId = "campus_social_issues"; // or "academic_success" or "tkam_themes"
   ```
   
   **Note**: Setting `forceQuestionSetId = "tkam_themes"` will use the original TKAM questions since "tkam_themes" doesn't exist as an ID in the expanded sets - the code falls back to the original file format.

### For Educators/Content Creators

1. **Adding New Question Sets**:
   ```json
   {
       "id": "your_theme_id",
       "name": "Your Theme Name", 
       "description": "Description of the question set",
       "difficulty": "easy/medium/hard",
       "weeks": [
           {
               "week": "1",
               "theme": "Theme Name",
               "questions": [
                   {
                       "question": "Question prompt",
                       "answer": "fiveletters", // Must be exactly 5 letters
                       "definition": "Definition or explanation"
                   }
               ]
           }
       ]
   }
   ```

2. **Question Writing Guidelines**:
   - All answers must be exactly 5 letters
   - Questions should relate to game themes
   - Include both question prompts and definitions
   - Consider difficulty progression

## Data Analysis Opportunities

### Research Questions You Can Now Answer:
1. **Educational Effectiveness**: Which question themes improve learning outcomes?
2. **Engagement Patterns**: Do campus-relevant questions increase engagement?
3. **Difficulty Optimization**: What question difficulty maximizes learning?
4. **Player Progression**: How does performance change over time?
5. **Content Preference**: Which themes resonate with different player demographics?

### Key Metrics Available:
- **Completion Rates**: By question set and player type
- **Learning Curves**: Accuracy improvement over time  
- **Engagement Time**: Session duration and return rates
- **Content Effectiveness**: Theme-specific learning outcomes

## Next Steps for Your Research

1. **Baseline Data Collection**: Run current implementation to establish baseline
2. **Theme Expansion**: Create additional question sets based on other game themes
3. **Adaptive Difficulty**: Use OGD data to adjust question difficulty dynamically
4. **Personalization**: Tailor question sets to individual player performance
5. **Longitudinal Studies**: Track learning retention over multiple sessions

## Technical Notes

- The system uses `SystemInfo.deviceUniqueIdentifier` for consistent A/B group assignment
- All logging is async and won't impact game performance
- Question sets are loaded at runtime, allowing for easy content updates
- The system is backwards compatible with existing save files

## Broader Themes Beyond TKAM

The new question sets cover themes that appear throughout Answer Campus:
- **Identity & Belonging**: Core to the university experience narrative
- **Ethics & Decision Making**: Central to many game choice scenarios  
- **Community & Relationships**: Fundamental to the social aspects
- **Academic Success**: Directly relevant to the educational setting