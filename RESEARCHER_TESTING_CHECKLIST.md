# Pre-Commit Testing Checklist for Researchers

## Before You Commit Any Changes - RUN THESE TESTS!

### ✅ **Step 1: Open Unity Project**
1. Open Unity Hub
2. Open the Answer-Campus project from `c:\Dev\NERDLab\Answer-Campus`
3. Wait for Unity to load completely

### ✅ **Step 2: Find or Create Test Scene**
**Option A: Use Existing Scene**
- Look in `Assets/Scenes/` for scenes containing the study game
- Common scene names: "Study", "Mini Games", "Main"

**Option B: Create Test Scene**
- File → New Scene
- Save as "Test_AB_System"

### ✅ **Step 3: Add Test Components**
1. **Right-click in Hierarchy** → Create Empty GameObject
2. **Rename to** "Test Runner"
3. **Add Component** → Search "TestABStudySystem"
4. **Configure:**
   - ✅ Run Tests On Start = True
   - ✅ Detailed Logging = True

### ✅ **Step 4: Connect Components** (If they exist in scene)
- **Question Loader**: Drag `EnhancedStudyQuestionLoader` from scene
- **Study Logger**: Drag `StudyGameLogger` from scene
- ⚠️ **If not found**: Tests will still validate JSON files and basic functionality

### ✅ **Step 5: Run Tests**
1. **Press Play button** in Unity
2. **Watch Console window** (Window → General → Console if not visible)
3. **Look for test results:**

### ✅ **Expected Test Results:**
```
[AB_TEST] === STARTING COMPREHENSIVE A/B STUDY SYSTEM TESTS ===
[AB_TEST] TEST 1: JSON File Loading
[AB_TEST] ✅ Original TKAM JSON file loaded successfully
[AB_TEST] ✅ Expanded JSON loaded: 2 question sets found
[AB_TEST] TEST 2: Answer Length Validation
[AB_TEST] ✅ All 20 answers are exactly 5 letters!
[AB_TEST] TEST 3: A/B Test Assignment
[AB_TEST] ✅ A/B Distribution: Control=4, Experimental=4
[AB_TEST] TEST 4: Question Set Selection
[AB_TEST] ✅ Questions loaded: 5 questions
[AB_TEST] TEST 5: OGD Logging
[AB_TEST] ✅ Session start logging successful
[AB_TEST] TEST 6: Forced Question Sets
[AB_TEST] ✅ Forced set 'tkam_themes': 5 questions loaded
[AB_TEST] === ALL TESTS COMPLETED ===
```

### ❌ **If You See Red Error Messages:**
**DO NOT COMMIT YET!**

Common issues and fixes:
1. **"Invalid answer length"** → Fix JSON file answers to be exactly 5 letters
2. **"JSON file not found"** → Check file paths in Resources/Data folder
3. **"No questions loaded"** → Check JSON file formatting
4. **"Component not found"** → Make sure study game components exist in scene

### ✅ **Step 6: Test Different Question Sets**
1. **In Inspector**, find the `EnhancedStudyQuestionLoader` component
2. **Set Force Question Set Id** to:
   - `"tkam_themes"` → Test original questions
   - `"campus_social_issues"` → Test campus questions  
   - `"academic_success"` → Test study skills questions
3. **Press Play** for each one
4. **Verify each loads without errors**
5. **Clear Force Question Set Id** when done

### ✅ **Step 7: Verify A/B Testing Works**
1. **Clear Force Question Set Id** (leave empty)
2. **Press Play multiple times**
3. **Check console** for: `"Player assigned to A/B test group: [control/experimental]"`
4. **Verify you see both groups** over multiple runs

### ✅ **Step 8: Final Validation**
**All these should show ✅ in console:**
- [ ] JSON files load successfully
- [ ] All answers are exactly 5 letters
- [ ] A/B test assignment works
- [ ] Questions load for each test group
- [ ] OGD logging functions work
- [ ] Forced question sets work

## ✅ **ONLY COMMIT WHEN ALL TESTS PASS!**

### **When Tests Pass:**
```bash
git add .
git commit -m "feat: Enhanced A/B testing system with validated question sets"
git push origin main
```

### **When Tests Fail:**
1. **Fix the issues** identified in console
2. **Re-run tests** until all pass
3. **Then commit**

## **Why This Matters for Your Research:**
- ✅ **Prevents data loss** from game crashes
- ✅ **Ensures valid A/B testing** for your study
- ✅ **Guarantees reproducible results** for publication
- ✅ **Saves weeks of work** by catching issues early

## **Emergency Contact:**
If tests fail and you can't fix them, **don't commit broken code**. 
Instead: Document the errors and seek help before proceeding.

---
**Remember: 5 minutes of testing now saves weeks of invalid research data later!**
