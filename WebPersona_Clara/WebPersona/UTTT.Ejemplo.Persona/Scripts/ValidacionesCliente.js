
function validacionDeDatosCliente() {
    var clave = document.getElementById("txtClaveUnica").value;
    var nombre = document.getElementById("txtNombre").value;
    var paterno = document.getElementById("txtAPaterno").value;
    var materno = document.getElementById("txtAMaterno").value;
    var e = document.getElementById("ddlSexo");
    var hermanos = document.getElementById("txtNumHermanos").value;
    var ddlSexo = e.options[e.selectedIndex].value;
    var valor = false;
    var mensaje = "";
    if (isNaN(clave) == true || !(/^\d{3}$/.test(clave))) {
        mensaje = "Ingresa Una Clave Unica De Solo Numeros Y Que Tenga Tres Numeros";
        valor = false;
    }
    else if (!(/^([a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{3,15})+$/.test(nombre))) {
        mensaje = "Ingresa Un Nombre";
        valor = false;
    }

    else if (!(/^([a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{3,15})+$/.test(paterno))) {
        mensaje = "Ingresa Un Apellido Paterno";
        valor = false;
    }
    else if (!(/^([a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]{3,15})+$/.test(materno))) {
        mensaje = "Ingresa Un Apellido Materno";
        valor = false;
    }
    else if (ddlSexo < 0) {
        mensaje = "Ingresa Un Sexo";
        valor = false;
    } else if (!(/^([0-9])*$/.test(hermanos))) {
        mensaje = "Numero de Hermanos Solo Acepta Numeros";
        valor = false;
    } else {
        valor = true;
    }

    if (valor == false) {
        alert(mensaje);
    }

    return valor;
}

