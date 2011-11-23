(defun my-compile-project (buffer)
  (myke-invoke "compile" buffer))

(defun my-rebuild-project (buffer)
  (myke-invoke "rebuild" buffer))

(defun my-run-project (buffer)
  (myke-invoke "run" buffer))

(defun my-repl-project (buffer)
  (myke-invoke "repl" buffer))

(defun my-test-project (buffer)
  (myke-invoke "test" buffer))

(defun my-commit-project (buffer)
  (myke-invoke "commit" buffer))

(defun my-logall-project (buffer)
  (myke-invoke "logall" buffer))

(defun my-logthis-project (buffer)
  (myke-invoke "logthis" buffer))

(defun my-pull-project (buffer)
  (myke-invoke "pull" buffer))

(defun my-push-project (buffer)
  (myke-invoke "push" buffer))
