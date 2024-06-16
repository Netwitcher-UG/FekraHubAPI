// MUI Imports
import Link from 'next/link'

import { useParams } from 'next/navigation'

import Grid from '@mui/material/Grid'
import Button from '@mui/material/Button'

// Component Imports
import ChildrenCard from '@components/children-card/card'
import { getLocalizedUrl } from '@/utils/i18n'

const UserListCards = ({ data }) => {
  return (
    <div>
      <Button
        variant='contained'
        component={Link}
        startIcon={<i className='ri-add-line' />}
        href={getLocalizedUrl('pages/parents/children/create-student', 'en')}
        className='is-full sm:is-auto'
      >
        Create Student
      </Button>

      <Grid container spacing={6}>
        {data.map((item, i) => (
          <Grid key={i} item xs={12} sm={6} md={3}>
            <ChildrenCard {...item.student} {...item.course} />
          </Grid>
        ))}
      </Grid>
    </div>
  )
}

export default UserListCards

// MUI Imports
import Link from 'next/link'

import { useParams } from 'next/navigation'

import Grid from '@mui/material/Grid'
import Button from '@mui/material/Button'

// Component Imports
import ChildrenCard from '@components/children-card/card'
import { getLocalizedUrl } from '@/utils/i18n'

const UserListCards = ({ data }) => {
  return (
    <div>
      <Button
        variant='contained'
        component={Link}
        startIcon={<i className='ri-add-line' />}
        href={getLocalizedUrl('pages/parents/children/create-student', 'en')}
        className='is-full sm:is-auto'
      >
        Create Student
      </Button>

      <Grid container spacing={6}>
        {data.map((item, i) => (
          <Grid key={i} item xs={12} sm={6} md={3}>
            <ChildrenCard {...item.student} {...item.course} />
          </Grid>
        ))}
      </Grid>
    </div>
  )
}

export default UserListCards

