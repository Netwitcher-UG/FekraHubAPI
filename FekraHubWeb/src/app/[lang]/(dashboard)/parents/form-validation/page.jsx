// Next Imports
import Link from 'next/link'

// MUI Imports
import Grid from '@mui/material/Grid'
import Typography from '@mui/material/Typography'

// Component Imports

import FormValidationAsyncSubmit from '@views/parents/form-validation/FormValidationAsyncSubmit'

const FormValidation = () => {
  return (
    <Grid container spacing={6} justifyContent={'center'}>
      <Grid item xs={12} md={6}>
        <FormValidationAsyncSubmit />
      </Grid>
    </Grid>
  )
}

export default FormValidation
