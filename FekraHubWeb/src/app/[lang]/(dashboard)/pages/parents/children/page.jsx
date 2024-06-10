// Next Imports
import Link from 'next/link'

// MUI Imports
import Grid from '@mui/material/Grid'
import Typography from '@mui/material/Typography'

// Component Imports

import RegisterChildren from '@views/pages/parents/children/RegisterChildren'

const FormValidation = () => {
  return (
    <Grid container spacing={6} justifyContent={'center'}>
      <Grid item xs={12} md={10}>
        <RegisterChildren />
      </Grid>
    </Grid>
  )
}

export default FormValidation
