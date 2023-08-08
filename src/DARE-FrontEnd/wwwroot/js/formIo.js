function renderForm(layOutURLOrString, formData, submitFuction, ReadOnly = false, divId = "FormId", redirectOverrideUrl) {
    renderForm.draftSubmissionData = "";
    var formReference

    if (formData != "" && formData != null && Object.prototype.toString.call(formData) === "[object String]") {
        formData = JSON.parse(formData);
    }
    if (ReadOnly == 'True') {
        layOut = layOutURLOrString;
        if (layOutURLOrString.startsWith("http") == false) {
            layOut = JSON.parse(layOutURLOrString);
        }

        Formio.createForm(document.getElementById(divId), layOut, { viewAsHtml: true, readOnly: true }).then(function (form) {
            form.submission = {
                data: formData
            };
            formReference = form;
        });
    } else {
        layOut = layOutURLOrString;
        if (layOutURLOrString.startsWith("http") == false) {
            layOut = JSON.parse(layOutURLOrString);
        }
        Formio.createForm(document.getElementById(divId), layOut).then(function (form) {
            form.submission = {
                data: formData
            };
            form.on('change', function () { // Saves current data in form for saving drafts / external submits
                renderForm.draftSubmissionData = JSON.stringify(form.submission.data);
            });
            formReference = form;
        });
    }

    function AddListenerTo() {
        externalSubmit.addEventListener("click", function SubmitExternal() {
            externalSubmit.removeEventListener("click", SubmitExternal);
            var Response = submitForm();
            if (Response == "BAD") {
                AddListenerTo();
            }
        });
    }

    var externalSubmit = document.getElementById("ExternalSubmit");
    if (externalSubmit != null && externalSubmit.getAttribute("click") !== "true") {
        AddListenerTo();
    }

    function submitForm() {
        if (formReference.checkValidity(null, true, null, false)) {
            Data = renderForm.draftSubmissionData;
            Formio.fetch(submitFuction,
                {
                    body: JSON.stringify(Data),
                    headers: { 'content-type': 'application/json' },
                    method: 'POST',
                    mode: 'cors'
                }).then(response => response.json())
                .then(function (response) {
                    if (response.redirectToUrl) {
                        window.location.href = response.redirectToUrl;
                        return "GOOD"
                    }
                    else {
                        console.log("Bad " + response.message);
                        return "BAD"
                    }
                });
        } else {
            return "BAD"
        }
    }
};