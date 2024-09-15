export function initialize(id, dotnetObject) {
    jQuery('#' + id).bootstrapDualListbox();

    jQuery('#' + id).on('change', function (e) {
        let value = jQuery(this).val();
        console.log(value);
        dotnetObject.invokeMethodAsync('OnChange', value);
    })
}

export function refresh(id) {
    jQuery('#' + id).bootstrapDualListbox('refresh', true);
}