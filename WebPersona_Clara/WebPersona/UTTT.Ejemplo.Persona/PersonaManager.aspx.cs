#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UTTT.Ejemplo.Linq.Data.Entity;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Collections;
using UTTT.Ejemplo.Persona.Control;
using UTTT.Ejemplo.Persona.Control.Ctrl;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Net.Configuration;
using System.Text.RegularExpressions;

#endregion

namespace UTTT.Ejemplo.Persona
{
    public partial class PersonaManager : System.Web.UI.Page
    {
        #region Variables

        private SessionManager session = new SessionManager();
        private int idPersona = 0;
        private UTTT.Ejemplo.Linq.Data.Entity.Persona baseEntity;
        private DataContext dcGlobal = new DcGeneralDataContext();
        private int tipoAccion = 0;
        private int idSexoC = 0;

        #endregion
        
        #region Eventos

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.Response.Buffer = true;
                this.session = (SessionManager)this.Session["SessionManager"];
                this.idPersona = this.session.Parametros["idPersona"] != null ?
                    int.Parse(this.session.Parametros["idPersona"].ToString()) : 0;
                if (this.idPersona == 0)
                {
                    this.baseEntity = new Linq.Data.Entity.Persona();
                    this.tipoAccion = 1;
                }
                else
                {
                    this.baseEntity = dcGlobal.GetTable<Linq.Data.Entity.Persona>().First(c => c.id == this.idPersona);
                    this.tipoAccion = 2;
                }

                if (!this.IsPostBack)
                {
                    if (this.session.Parametros["baseEntity"] == null)
                    {
                        this.session.Parametros.Add("baseEntity", this.baseEntity);
                    }
                    List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().ToList();
                    CatSexo catTemp = new CatSexo();
                    catTemp.id = -1;
                    catTemp.strValor = "Seleccionar";
                    lista.Insert(0, catTemp);
                    this.ddlSexo.DataTextField = "strValor";
                    this.ddlSexo.DataValueField = "id";
                    this.ddlSexo.DataSource = lista;
                    this.ddlSexo.DataBind();
                    this.ddlSexo.SelectedIndexChanged += new EventHandler(ddlSexo_SelectedIndexChanged);
                    this.ddlSexo.AutoPostBack = true;
                    if (this.idPersona == 0)
                    {
                        this.lblAccion.Text = "Agregar";
                        this.txtFecha.Value = null;
                    }
                    else
                    {
                        this.lblAccion.Text = "Editar";
                        this.txtNombre.Text = this.baseEntity.strNombre;
                        this.txtAPaterno.Text = this.baseEntity.strAPaterno;
                        this.txtAMaterno.Text = this.baseEntity.strAMaterno;
                        this.txtClaveUnica.Text = this.baseEntity.strClaveUnica;
                        DateTime? fechaNacimiento = this.baseEntity.strFechaNacimiento;
                        this.txtFecha.Value = fechaNacimiento.ToString();
                        this.dteCalendar.TodaysDate = (DateTime)fechaNacimiento;
                        this.dteCalendar.SelectedDate = (DateTime)fechaNacimiento;
                        this.txtNumHermanos.Text = this.baseEntity.intNumHermano.ToString();
                        this.txtCorreoElectronico.Text = this.baseEntity.strCorreo;
                        this.txtCodigoPostal.Text = this.baseEntity.strCorreo;
                        this.txtRFC.Text = this.baseEntity.strRFC;
                        this.setItem(ref this.ddlSexo, baseEntity.CatSexo.strValor);
                        ddlSexo.Items.FindByValue("-1").Enabled = false;
                      
                    }                
                }

            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un problema al cargar la página");
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
            }

        }

        

        

        protected void ddlSexo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                int idSexo = int.Parse(this.ddlSexo.Text);
                Expression<Func<CatSexo, bool>> predicateSexo = c => c.id == idSexo;
                predicateSexo.Compile();
                List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().Where(predicateSexo).ToList();
                CatSexo catTemp = new CatSexo();            
                this.ddlSexo.DataTextField = "strValor";
                this.ddlSexo.DataValueField = "id";
                this.ddlSexo.DataSource = lista;
               // this.ddlSexo.DataBind();
               
            }
            catch (Exception)
            {
                this.showMessage("Ha ocurrido un error inesperado");
            }
        }

        #endregion
       
        #region Metodos

        public void setItem(ref DropDownList _control, String _value)
        {
            foreach (ListItem item in _control.Items)
            {
                if (item.Value == _value)
                {
                    item.Selected = true;
                    break;
                }
            }
            _control.Items.FindByText(_value).Selected = true;
        }

        #endregion

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
               

                DataContext dcGuardar = new DcGeneralDataContext();
                UTTT.Ejemplo.Linq.Data.Entity.Persona persona = new Linq.Data.Entity.Persona();
                if (this.idPersona == 0)
                {
                    persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                    persona.strNombre = this.txtNombre.Text.Trim();
                    persona.strAMaterno = this.txtAMaterno.Text.Trim();
                    persona.strAPaterno = this.txtAPaterno.Text.Trim();
                    persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                    DateTime FechaNacimiento = this.dteCalendar.SelectedDate.Date;
                    persona.strFechaNacimiento = FechaNacimiento;
                    persona.intNumHermano = int.Parse(this.txtNumHermanos.Text.Trim());
                    persona.strCorreo = this.txtCorreoElectronico.Text.Trim();
                    persona.strCodigoPostal = int.Parse(this.txtCodigoPostal.Text.Trim());
                    persona.strRFC = this.txtRFC.Text.Trim();

                    //operador ternario


                    String mensaje = String.Empty;
                    //Validacion de datos correctos

                    if (!this.email_bien_escrito(persona, ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }

                    if (!this.validacion(persona, ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }


                    if (!this.validaSql(ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }

                    if (!this.validaHTML(ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }



                    dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().InsertOnSubmit(persona);
                    dcGuardar.SubmitChanges();
                    this.showMessage("El registro se agrego correctamente.");
                    this.Response.Redirect("~/PersonaPrincipal.aspx", false);

                }
                if (this.idPersona > 0)
                {

                    persona = dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().First(c => c.id == idPersona);
                    persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                    persona.strNombre = this.txtNombre.Text.Trim();
                    persona.strAMaterno = this.txtAMaterno.Text.Trim();
                    persona.strAPaterno = this.txtAPaterno.Text.Trim();
                    persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                    DateTime fechaNacimiento = this.dteCalendar.SelectedDate.Date;
                    persona.strFechaNacimiento = fechaNacimiento;
                    persona.intNumHermano = int.Parse(this.txtNumHermanos.Text.Trim());
                    persona.strCorreo = this.txtCorreoElectronico.Text.Trim();
                    persona.strCodigoPostal = int.Parse(this.txtCodigoPostal.Text.Trim());
                    persona.strRFC = this.txtRFC.Text.Trim();

                    String mensaje = String.Empty;
                    //Validacion de datos correctos

                    if (!this.email_bien_escrito(persona, ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }

                    if (!this.validacion(persona, ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }


                    if (!this.validaSql(ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }

                    if (!this.validaHTML(ref mensaje))
                    {
                        this.lblMensaje.Text = mensaje;
                        this.lblMensaje.Visible = true;
                        return;
                    }

                    dcGuardar.SubmitChanges();
                    this.showMessage("El registro se edito correctamente.");
                    this.Response.Redirect("~/PersonaPrincipal.aspx", false);
                }
            }
            catch (Exception _e)
            {

                //Obtenemos el servidor smtp del archivo de configuración.
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string strHost = smtpSection.Network.Host;
                int port = smtpSection.Network.Port;
                string strUserName = smtpSection.Network.UserName;
                string strFromPass = smtpSection.Network.Password;

                //Proporcionamos la información de autenticación al servidor de Gmail
                SmtpClient smtp = new SmtpClient(strHost, port);
                MailMessage msg = new MailMessage();

                //Creamos el contenido del correo. 
                string body = "<h1>Error" + _e.Message + "</h1>";
                msg.From = new MailAddress(smtpSection.From, "TRABAJO");
                msg.To.Add(new MailAddress("18300871@uttt.edu.mx"));
                msg.Subject = "Correo";
                msg.IsBodyHtml = true;
                msg.Body = body;

                //Enviamos el correo
                smtp.Credentials = new NetworkCredential(strUserName, strFromPass);
                smtp.EnableSsl = true;
                smtp.Send(msg);

                Response.Redirect("~/ErrorPage.aspx", false);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un error inesperado");
            }
        }


        public bool validacion(UTTT.Ejemplo.Linq.Data.Entity.Persona _persona, ref String _mensaje)
        {

            // Validacion combo 
            if(_persona.idCatSexo == -1)
            {
                _mensaje = "Seleccione Masculino o Femenino";
                return false;
            }

            // Validacion Clave unica
            
            int i = 0;
            if(int.TryParse(_persona.strClaveUnica, out i) == false)
            {
                _mensaje = "La clave unica no es numero";
                return false;
            }
            if (_persona.strClaveUnica.Equals(String.Empty))
            {
                _mensaje = "Clave Unica esta vacio";
                return false;
            }
            if (int.Parse(_persona.strClaveUnica) < 100 || int.Parse(_persona.strClaveUnica) > 999)
            {
                _mensaje = "La clave debe de constar de 3 numeros";
                return false;
            }
            

            // Validacion Nombre
            if(int.TryParse(_persona.strNombre, out i) == true)
            {
                _mensaje = "No se permiten numeros en nombre";
                return false;
            }

            if ( _persona.strNombre.Equals(String.Empty))
            {
                _mensaje = "Nombre esta vacio";
                return false;
            }
            if(_persona.strNombre.Length > 50)
            {
                _mensaje = "Los caracteres permitidos para nombre rebasan lo establecido";
                return false;
            }
            if(_persona.strNombre.Length <= 2)
            {
                _mensaje = "Ingrese un nombre valido";
                return false;
            }

            // Validacion Apellido Paterno
            if (int.TryParse(_persona.strNombre, out i) == true)
            {
                _mensaje = "No se permiten numeros en apellido paterno";
                return false;
            }

            if (_persona.strAPaterno.Equals(String.Empty))
            {
                _mensaje = "Apellido Paterno esta vacio";
                return false;
            }
            if (_persona.strAPaterno.Length > 50)
            {
                _mensaje = "Los caracteres permitidos para Apellido Paterno rebasan lo establecido";
                return false;
            }
            if(_persona.strAPaterno.Length <= 2)
            {
                _mensaje = "Ingrese un apellido paterno valido";
                return false;
            }

            // Validacion Apellido Materno
            if (int.TryParse(_persona.strNombre, out i) == true)
            {
                _mensaje = "No se permiten numeros en apellido materno";
                return false;
            }
            if (_persona.strAMaterno.Equals(String.Empty))
            {
                _mensaje = "Apellido Materno esta vacio";
                return false;
            }
            if (_persona.strAPaterno.Length > 50)
            {
                _mensaje = "Los caracteres permitidos para Apellido Materno rebasan lo establecido";
                return false;
            }
            if(_persona.strAMaterno.Length <= 2)
            {
                _mensaje = "Ingrese un apellido materno valido";
                return false;
            }

            // Validacion Numero de hermanos
            if (_persona.intNumHermano.Equals(String.Empty))
            {
                _mensaje = "Numero de hermanos esta vacio";
                return false;
            }
            if (int.TryParse( _persona.intNumHermano.ToString(), out i) == false)
            {
                _mensaje = "Numero de hermanos no es numero";
                return false;
            }
            if (_persona.intNumHermano < 0 || _persona.intNumHermano > 20 )
            {
                _mensaje = "Los numeros de hermanos no deben ser menores a cero";
                return false;
            }
           

            // Validacion Correo Electronico
            if (_persona.strCorreo.Equals(String.Empty))
            {
                _mensaje = "Correo Electronico esta vacio";
                return false;
            }
            if (_persona.strCorreo.Length > 50)
            {
                _mensaje = "Los caracteres permitidos para Correo Electronico rebasan lo establecido";
                return false;
            }


            // Validacion RFC Empresa
            if (_persona.strRFC.Equals(String.Empty))
            {
                _mensaje = "RFC esta vacio";
                return false;
            }
            if (_persona.strRFC.Length > 50)
            {
                _mensaje = "Los caracteres permitidos para RFC rebasan lo establecido";
                return false;
            }

            // Validar Codigo Postal
            //if (int.TryParse(_persona.strCodigoPostal, out i) == false)
            //{
            //    _mensaje = "El codigo postal no es numero";
            //    return false;
            //}
            //if (_persona.strCodigoPostal.Equals(String.Empty))
            //{
            //    _mensaje = "Codigo Postal esta vacio";
            //    return false;
            //}
            //if (_persona.strCodigoPostal.Length < 5)
            //{
            //    _mensaje = "El codigo postal debe de constar de 5 numeros";
            //    return false;
            //}
            //if ( _persona.strCodigoPostal == "00000")
            //{
            //    _mensaje = "El codigo postal no valido";
            //    return false;
            //}

            // Validacion Calendario
            TimeSpan timeSpan = DateTime.Now - _persona.strFechaNacimiento.Value.Date;
            if(timeSpan.Days < 6570)
            {
                _mensaje = "La persona es menor de edad";
                return false;
            }
            if(timeSpan.Days >= 737821)
            {
                _mensaje = "Ingrese una fecha en el calendario";
                return false;
            }
            
            return true;
        }

        private bool validaSql(ref String _mensaje)
        {
            CtrlValidaInyeccion valida = new CtrlValidaInyeccion();
            string mensajeFuncion = string.Empty;
            if (valida.sqlInyectionValida(this.txtNombre.Text.Trim(), ref mensajeFuncion, "Nombre", ref this.txtNombre))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.sqlInyectionValida(this.txtAPaterno.Text.Trim(), ref mensajeFuncion, "A Paterno", ref this.txtAPaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.sqlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensajeFuncion, "A Materno", ref this.txtAMaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.sqlInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensajeFuncion, "Correo Electronico", ref this.txtCorreoElectronico))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.sqlInyectionValida(this.txtRFC.Text.Trim(), ref mensajeFuncion, "RFC", ref this.txtRFC))
            {
                _mensaje = mensajeFuncion;
                return false;
            }

            return true;
        }

        private bool validaHTML(ref String _mensaje)
        {
            CtrlValidaInyeccion valida = new CtrlValidaInyeccion();
            string mensajeFuncion = string.Empty;
            if(valida.htmlInyectionValida(this.txtNombre.Text.Trim(), ref mensajeFuncion, "Nombre", ref this.txtNombre))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAPaterno.Text.Trim(), ref mensajeFuncion, "A Paterno", ref this.txtAPaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensajeFuncion, "A Materno", ref this.txtAMaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensajeFuncion, "Correo Electronico", ref this.txtCorreoElectronico))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtRFC.Text.Trim(), ref mensajeFuncion, "RFC", ref this.txtRFC))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtClaveUnica.Text.Trim(), ref mensajeFuncion, "Clave Unica", ref this.txtClaveUnica))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCodigoPostal.Text.Trim(), ref mensajeFuncion, "Codigo Postal", ref this.txtCodigoPostal))
            {
                _mensaje = mensajeFuncion;
                return false;
            }


            return true;
        }

        // Calendario
        protected void dteCalendar_SelectionChanged1(object sender, EventArgs e)
        {
            txtFecha.Value = dteCalendar.SelectedDate.ToString();

        }

        private Boolean email_bien_escrito(UTTT.Ejemplo.Linq.Data.Entity.Persona _persona, ref String _mensaje)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(_persona.strCorreo, expresion))
            {
                if (Regex.Replace(_persona.strCorreo, expresion, String.Empty).Length == 0)
                {

                    return true;
                }
                else
                {
                    _mensaje = "Correo Electronico No Valido";
                    
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}