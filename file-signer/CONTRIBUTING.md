# CD/DVD Build Automation Contributing


## Tips / Coding Convention
* Use project settings.json and VS code editor
* For method names use lowercase + underscore. see example below:

    ```
        def get_latest(self, arg): 
            return 0 
    ```

* For file-naming, use lowercase and do NOT use dash (-). see example below:

    ```
        getlatest.py
        isowrapper.py
        build.py
    ```

* For classes, only first letter is capitalized. see example below

    ```
        class Generalbuildemail
        class P4wrapper
    ```
    
* Use OOP if possible.
* IMPORTANT: Before using 'git commit', use the following procedure. These steps are to prevent auto-merging commits by git.
    1.  git stash (so that your working copy will be clean)
    2.  git pull (to update to latest version)
    3.  git stash pop (to bring back your changes, then merge your changes if there are conflicts)
    4.  git commit 
    5.  git push 

    
