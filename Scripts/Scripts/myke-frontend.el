(defun my-compile-project (buffer &optional callback)
  (myke-invoke "compile" buffer &optional callback))

(defun my-clean-project (buffer &optional callback)
  (myke-invoke "clean" buffer &optional callback))

(defun my-rebuild-project (buffer &optional callback)
  (myke-invoke "rebuild" buffer &optional callback))

(defun my-run-project (buffer &optional callback)
  (myke-invoke "run" buffer &optional callback))

(defun my-repl-project (buffer &optional callback)
  (myke-invoke "repl" buffer &optional callback))

(defun my-run-test-project (buffer &optional callback)
  (myke-invoke "run-test" buffer &optional callback))

(defun my-commit-project (buffer &optional callback)
  (myke-invoke "commit" buffer &optional callback))

(defun my-logall-project (buffer &optional callback)
  (myke-invoke "logall" buffer &optional callback))

(defun my-logthis-project (buffer &optional callback)
  (myke-invoke "logthis" buffer &optional callback))

(defun my-pull-project (buffer &optional callback)
  (myke-invoke "pull" buffer &optional callback))

(defun my-push-project (buffer &optional callback)
  (myke-invoke "push" buffer &optional callback))