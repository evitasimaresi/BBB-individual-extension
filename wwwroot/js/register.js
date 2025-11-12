document.getElementById('registerForm').addEventListener('submit', function (e) {
  e.preventDefault();
  const form = e.target;

  if (form.checkValidity()) {
    // so the pop-up doesn't activate if the reigstration field inputs are wrong
    const modal = new bootstrap.Modal(document.getElementById('dialog-box'));
    modal.show();
  } else {
    form.reportValidity();
  }
});
